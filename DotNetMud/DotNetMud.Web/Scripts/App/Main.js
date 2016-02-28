// http://stackoverflow.com/questions/2504568/javascript-namespace-declaration
var spaceMud = (function (spaceMud) {

    // Global / accessible to stuff defined here. 
    // Get handles on external things. 
    // Declare a proxy to reference the hub.

    // COORDINATE SYSTEM
    //              -Y
    // 
    //   -X        0,0       +X
    //                 \
    //              +Y  \  45 degrees
    //   +R = to the right, clockwise
    //   -R = to the left, counter clockwise

    // IMAGE:  
    // 
    //   FRONT faces top of image
    //   
    //       ^
    //     ^=|=^
    

    var chat = $.connection.spaceHub;
    var canvas = document.getElementById("canvas");
    var spanMsBetweenServerUpdates = document.getElementById("msBetweenServerUpdates");
    var spanMsBetweenAnimates = document.getElementById("msBetweenAnimates");
    var spanMsToAnimate = document.getElementById("msToAnimate");
    var spanLocationX = document.getElementById("locationX");
    var spanLocationY = document.getElementById("locationY");
    var spanSpeed = document.getElementById("speed");
    var spanMeHasBeenHitCount = document.getElementById("meHasBeenHitCount");
    var spanMeHasHitSomeoneCount = document.getElementById("meHasHitSomeoneCount");
    var context = canvas.getContext("2d");
    context.font = "20px Arial";

    var serverObjects = {
        Me: { X: 0, Y: 0, DX: 0, DY: 0, R: 0, DR: 0, Name: "", Image: "" },
        Others: [],
        ServerTimeInSeconds: 0,
        ServerTimeRate: 1
    };

    var keyLastDownAt = {};  // high performance counter
    var keyPressedLength = {};  // cumulates if not pressed

    spaceMud.keyUpHandler = function() {
        var kc = event.keyCode;

        var ld = keyLastDownAt[kc];
        keyLastDownAt[kc] = 0;
        if (ld) {
            var now = window.performance.now(); 
            var ela = now - ld;
            keyPressedLength[kc] = (keyPressedLength[kc]||0) + ela;
        }
    }

    spaceMud.getKeyPressedLengthAndReset = function(kc) {
        var l1 = keyPressedLength[kc] || 0;
        keyPressedLength[kc] = 0;
        var ld = keyLastDownAt[kc] || 0;
        if (ld) {
            // key is currently down, so read the value and reset it to look like it was just pressed
            var now = window.performance.now(); 
            var ela = now - ld;
            l1 = l1 + ela;
            keyLastDownAt[kc] = now;
        }
        return l1; 
    }

    spaceMud.keyDownHandler = function () {
        var keyCode = event.keyCode;
        if (!keyLastDownAt[keyCode]) {
            keyLastDownAt[keyCode] = window.performance.now();
        }
    }

    var clientInfo = {
        MyTimeAtServerTimeInMs: window.performance.now()
    };

    var loadedImages = {};

    spaceMud.getImageForUrl = function (url) {
        if (!url) return {};
        var x = loadedImages[url];
        if (!x) {
            var image = new Image();
            image.src = url;
            loadedImages[url] = image;
            return image;
        } else {
            return loadedImages[url];
        }
    };

    chat.client.serverSendsPollResultToClient = function (data) {

        // TODO: make this less chatty .. only things that update get sent. and shorter names
        // pong! 

        serverObjects.Me.X = data.Me.X;
        serverObjects.Me.Y = data.Me.Y;
        serverObjects.Me.R = data.Me.R;
        serverObjects.Me.DX = data.Me.DX;
        serverObjects.Me.DY = data.Me.DY;
        serverObjects.Me.DR = data.Me.DR; 
        serverObjects.Me.Image = data.Me.Image;
        serverObjects.ServerTimeInSeconds = data.ServerTimeInSeconds;
        serverObjects.ServerTimeRate = data.ServerTimeRate;
        spanMeHasBeenHitCount.textContent = data.MeHasBeenHitCount;
        spanMeHasHitSomeoneCount.textContent = data.MeHasHitSomeoneCount; 

        var previousMyTime = clientInfo.MyTimeAtServerTimeInMs; 
        clientInfo.MyTimeAtServerTimeInMs = window.performance.now();
        clientInfo.Velocity = Math.sqrt(serverObjects.Me.DX * serverObjects.Me.DX + serverObjects.Me.DY * serverObjects.Me.DY);

        spanSpeed.textContent = clientInfo.Velocity.toFixed(0);  

        var msBetweenServerUpdates = clientInfo.MyTimeAtServerTimeInMs - previousMyTime;
        spanMsBetweenServerUpdates.textContent = msBetweenServerUpdates.toFixed(0);
            
        serverObjects.Others = data.Others;

        spaceMud.doClientRequestsPollFromServer();  // ping! 
    }

    spaceMud.drawDotBackground = function(context, shipx, shipy) {
        context.save();
        {
            var w = context.canvas.width / 2 + 200;
            var h = context.canvas.height / 2 + 200;
            var absStartX = shipx - w;
            var absStartY = shipy - h;
            absStartX = Math.floor(absStartX / 100) * 100;
            absStartY = Math.floor(absStartY / 100) * 100;
            for (var x = absStartX; x < shipx + w; x += 100) {
                for (var y = absStartY; y < shipy + h; y += 100) {
                    context.fillRect(x - shipx, y - shipy, 1, 1);
                }
            }
        }
        context.restore();
    }; 

    var lastAnimate = 0; 
    spaceMud.animate = function animate(timestamp) {
        requestAnimationFrame(spaceMud.animate);

        var elapsed = timestamp - lastAnimate;
        lastAnimate = timestamp;
        spanMsBetweenAnimates.textContent = elapsed.toFixed(0); 

        var now = window.performance.now();  // should be the same as timestamp.. almost
        clientInfo.ElapsedSecondsSinceServerRefresh =
        (now - clientInfo.MyTimeAtServerTimeInMs) / (serverObjects.ServerTimeRate * 1000);

        // from: http://stackoverflow.com/questions/10214873/make-canvas-as-wide-and-as-high-as-parent 
        // Make it visually fill the positioned parent
        canvas.style.width = "100%";
        canvas.style.height = "100%";
        // ...then set the internal size to match
        canvas.width = canvas.offsetWidth;
        canvas.height = canvas.offsetHeight;

        context.clearRect(0, 0, canvas.width, canvas.height);
        context.save();
        {
            // drawing everything.  make 0,0 the center of the screen
            context.translate(canvas.width / 2, canvas.height / 2);

            // drawing me last so i show on top of everyone else
            var me = serverObjects.Me;
            if (me) {

                var me2 = {
                    X: me.X + me.DX * clientInfo.ElapsedSecondsSinceServerRefresh,
                    Y: me.Y + me.DY * clientInfo.ElapsedSecondsSinceServerRefresh,
                    R: me.R + me.DR * clientInfo.ElapsedSecondsSinceServerRefresh
                };
                spanLocationX.textContent = me2.X.toFixed(0);
                spanLocationY.textContent = me2.Y.toFixed(0); 

                spaceMud.drawDotBackground(context, me2.X, me2.Y);

                // drawing everything else first so i can draw myself over everything later
                for (var i = 0; i < serverObjects.Others.length; i++) {
                    var ob = serverObjects.Others[i];
                    if (ob) {
                        context.save();
                        {
                            var ob2 = {
                                X: ob.X + ob.DX * clientInfo.ElapsedSecondsSinceServerRefresh,
                                Y: ob.Y + ob.DY * clientInfo.ElapsedSecondsSinceServerRefresh,
                                R: ob.R + ob.DR * clientInfo.ElapsedSecondsSinceServerRefresh
                            }
                            context.translate(ob2.X-me2.X, ob2.Y-me2.Y);
                            var theirImage = spaceMud.getImageForUrl(ob.Image);
                            if (theirImage.complete) {
                                context.save();
                                {
                                    context.rotate((ob2.R+90) * Math.PI / 180);
                                    context.drawImage(theirImage, -theirImage.width / 2, -theirImage.height / 2);
                                }
                                context.restore();
                            }
                            context.fillText(ob.Name, 0, 0);
                        }
                        context.restore();
                    }
                }

                // draw me last because i need to be on top of everything else. 
                var myShipImage = spaceMud.getImageForUrl(me.Image);
                if (myShipImage.complete) {
                    context.save();
                    {
                        context.rotate((me2.R+90) * Math.PI / 180);
                        context.drawImage(myShipImage, -myShipImage.width / 2, -myShipImage.height / 2);
                    }
                    context.restore();
                }
                context.fillText("Me", 0, 0);
            }

        }
        context.restore();

        var now2 = window.performance.now();
        var elapsed2 = now2 - now;
        spanMsToAnimate.textContent = elapsed2.toFixed(0);

    };

    spaceMud.doClientRequestsPollFromServer = function() {
        var thrust = 0, left = 0, right = 0, fire = 0;
        thrust = spaceMud.getKeyPressedLengthAndReset(87);
        left = spaceMud.getKeyPressedLengthAndReset(65);
        right = spaceMud.getKeyPressedLengthAndReset(68);
        fire = spaceMud.getKeyPressedLengthAndReset(32);

        // HACK: temporarily turning on auto-fire
        chat.server.clientRequestsPollFromServer(thrust, left, right, fire);  // ping! 
    }

    spaceMud.main = function () {
        //Set the hubs URL for the connection
        $.connection.hub.url = window.location.origin + "/signalr";

        // Start the connection.
        $.connection.hub.start().done(function () {
            spaceMud.doClientRequestsPollFromServer();
            requestAnimationFrame(spaceMud.animate);

            document.addEventListener("keydown", spaceMud.keyDownHandler, false);
            document.addEventListener("keyup", spaceMud.keyUpHandler, false);
        });
    };

    return spaceMud;
}(spaceMud || {}));


spaceMud.main();
