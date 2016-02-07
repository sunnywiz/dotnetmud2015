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
    var context = canvas.getContext("2d");
    context.font = "20px Arial";

    var serverObjects = {
        Me: { X: 0, Y: 0, DX: 0, DY: 0, R: 0, DR: 0, Name: "", Image: "" },
        Others: []
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

        // some smart updating needs to happen here. 
        // example: load images on image change, but not otherwise. 

        serverObjects.Me.X = data.Me.X;
        serverObjects.Me.Y = data.Me.Y;
        serverObjects.Me.R = data.Me.R;
        serverObjects.Me.Image = data.Me.Image;

        serverObjects.Others = data.Others;

        // and go again. 
        chat.server.clientRequestsPollFromServer();
    }

    spaceMud.animate = function animate(timestamp) {

        requestAnimationFrame(spaceMud.animate);

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

                // drawing everything else
                for (var i = 0; i < serverObjects.Others.length; i++) {
                    var ob = serverObjects.Others[i];
                    if (ob) {
                        context.save();
                        {
                            context.translate(ob.X-me.X, ob.Y-me.Y);
                            var theirImage = spaceMud.getImageForUrl(ob.Image);
                            if (theirImage.complete) {
                                context.save();
                                {
                                    context.rotate((ob.R+90) * Math.PI / 180);
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
                        context.rotate((me.R+90) * Math.PI / 180);
                        context.drawImage(myShipImage, -myShipImage.width / 2, -myShipImage.height / 2);
                    }
                    context.restore();
                }
                context.fillText("Me", 0, 0);
            }

        }
        context.restore();
    };

    spaceMud.main = function () {
        //Set the hubs URL for the connection
        $.connection.hub.url = "http://localhost:30518/signalr";

        // Start the connection.
        $.connection.hub.start().done(function () {
            chat.server.clientRequestsPollFromServer();  
            requestAnimationFrame(spaceMud.animate);
        });
    };

    return spaceMud;
}(spaceMud || {}));


spaceMud.main();
