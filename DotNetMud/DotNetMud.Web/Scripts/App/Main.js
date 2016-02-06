﻿$(function() {
    //Set the hubs URL for the connection
    $.connection.hub.url = "http://localhost:30518/signalr";

    // Get handles on external things. 
    // Declare a proxy to reference the hub.
    var chat = $.connection.spaceHub;
    var canvas = document.getElementById('canvas');
    var context = canvas.getContext('2d');
    context.font = "20px Arial";

    var shipImage1 = new Image();
    shipImage1.src = "http://localhost:30518/Content/ship1.png";

    var gameObjects = {
        Me: { X: 0, Y: 0, DX: 0, DY: 0, R: 0, DR: 0, Name: '', Image: '' },
        Others: []
    };

    chat.client.serverSendsPollResultToClient = function(data) {

        // some smart updating needs to happen here. 
        // example: load images on image change, but not otherwise. 

        gameObjects.Me.X = data.Me.X;
        gameObjects.Me.Y = data.Me.Y;
        gameObjects.Me.R = data.Me.R;

        gameObjects.Others = data.Others;
    }

    function animate(timestamp) {

        requestAnimationFrame(animate);

        var desiredWidth = window.innerWidth - 100;
        var desiredHeight = window.innerHeight - 100;

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
            var me = gameObjects.Me;
            if (shipImage1.complete) {
                context.save();
                {
                    // drawing me
                    context.rotate((180 - me.R) * Math.PI / 180);
                    context.drawImage(shipImage1, -shipImage1.width / 2, -shipImage1.height / 2);
                }
                context.restore();
                context.fillText(0, 0, "Me");
            }
            for (var i = 0; i < gameObjects.Others.length; i++) {
                var ob = gameObjects.Others[i];
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
                            context.fillText(0, 0, ob.Name);
                        }
                        context.restore();
                    }
                }
            }
        }

        context.restore();
    }

    // Set initial focus to message input box.
    $("#message").focus();
    // Start the connection.
    $.connection.hub.start().done(function() {
        setInterval(function() { chat.server.clientRequestsPollFromServer(); }, 1000);
        requestAnimationFrame(animate);
    });
});
