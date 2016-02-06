// http://stackoverflow.com/questions/2504568/javascript-namespace-declaration
var spaceMud = (function(spaceMud) {

    // Global / accessible to stuff defined here. 
    // Get handles on external things. 
    // Declare a proxy to reference the hub.

    var chat = $.connection.spaceHub;
    var canvas = document.getElementById("canvas");
    var context = canvas.getContext("2d");
    context.font = "20px Arial";

    var serverObjects = {
        Me: { X: 0, Y: 0, DX: 0, DY: 0, R: 0, DR: 0, Name: "", Image: "" },
        Others: []
    };
    var shipImage1; 


    chat.client.serverSendsPollResultToClient = function (data) {

        // some smart updating needs to happen here. 
        // example: load images on image change, but not otherwise. 

        serverObjects.Me.X = data.Me.X;
        serverObjects.Me.Y = data.Me.Y;
        serverObjects.Me.R = data.Me.R;

        serverObjects.Others = data.Others;
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
            // drawing everything
            context.translate(canvas.width / 2, canvas.height / 2);
            var me = serverObjects.Me;
            if (shipImage1.complete) {
                context.save();
                {
                    // drawing me
                    context.rotate((180 - me.R) * Math.PI / 180);
                    context.drawImage(shipImage1, -shipImage1.width / 2, -shipImage1.height / 2);
                }
                context.restore();
                context.fillText("Me", 0, 0);
            }
            for (var i = 0; i < serverObjects.Others.length; i++) {
                var ob = serverObjects.Others[i];
                if (ob !== null) {
                    if (shipImage1.complete) {
                        context.save();
                        {
                            context.translate(me.X - ob.X, me.Y - ob.Y);
                            context.save();
                            {
                                context.rotate((180 - ob.R) * Math.PI / 180);
                                context.drawImage(shipImage1, -shipImage1.width / 2, -shipImage1.height / 2);
                            }
                            context.restore();
                            context.fillText(ob.Name, 0, 0);
                        }
                        context.restore();
                    }
                }
            }
        }

        context.restore();
    };

    spaceMud.main = function () {
        //Set the hubs URL for the connection
        $.connection.hub.url = "http://localhost:30518/signalr";

        shipImage1 = new Image();
        shipImage1.src = "http://localhost:30518/Content/ship1.png";

        // Start the connection.
        $.connection.hub.start().done(function () {
            setInterval(function () { chat.server.clientRequestsPollFromServer(); }, 1000);
            requestAnimationFrame(spaceMud.animate);
        });
    };

    return spaceMud; 
}(spaceMud || {}));
    
    
spaceMud.main(); 
