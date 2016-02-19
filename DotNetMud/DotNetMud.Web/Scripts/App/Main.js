﻿// http://stackoverflow.com/questions/2504568/javascript-namespace-declaration
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
        clientInfo.MyTimeAtServerTimeInMs = window.performance.now();

        serverObjects.Others = data.Others;

        spaceMud.doClientRequestsPollFromServer();  // ping! 
    }

    var lastAnimate = 0; 
    spaceMud.animate = function animate(timestamp) {

        requestAnimationFrame(spaceMud.animate);

        var now = window.performance.now();  // should be the same as timestamp.. almost
        clientInfo.ElapsedSecondsSinceServerRefresh =
        (now - clientInfo.MyTimeAtServerTimeInMs) / (serverObjects.ServerTimeRate * 1000);

        var desiredWidth = 700; // window.innerWidth - 200;
        var desiredHeight = 500; // window.innerHeight - 200;

        if (context.canvas.width !== desiredWidth) {
            context.canvas.width = desiredWidth;
        }
        if (context.canvas.height !== desiredHeight) {
            context.canvas.height = desiredHeight;
        }

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

                // drawing everything else
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
    };

    spaceMud.doClientRequestsPollFromServer = function() {
        var thrust = 0, left = 0, right = 0;
        thrust = spaceMud.getKeyPressedLengthAndReset(87);
        left = spaceMud.getKeyPressedLengthAndReset(65);
        right = spaceMud.getKeyPressedLengthAndReset(68);

        chat.server.clientRequestsPollFromServer(thrust, left, right);  // ping! 
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
