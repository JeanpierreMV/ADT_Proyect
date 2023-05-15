window.addEventListener('DOMContentLoaded', function () {
    // Obtén el elemento del lienzo (canvas) por su ID
    var canvas = document.getElementById("renderCanvas");

    // Crea el motor de Babylon.js
    var engine = new BABYLON.Engine(canvas, true);

    // Crea la escena
    var createScene = function () {
        // Crea una escena vacía
        var scene = new BABYLON.Scene(engine);

        // Crea una cámara y colócala en la posición deseada
        var camera = new BABYLON.FreeCamera("camera", new BABYLON.Vector3(0, 5, -10), scene);

        // Apunta la cámara hacia el origen de la escena
        camera.setTarget(BABYLON.Vector3.Zero());

        // Habilita los controles de movimiento de la cámara
        camera.attachControl(canvas, true);

        // Agrega luces a la escena
        var light = new BABYLON.HemisphericLight("light", new BABYLON.Vector3(0, 1, 0), scene);

        // Crea una esfera en la escena
        var sphere = BABYLON.MeshBuilder.CreateSphere("sphere", { diameter: 2 }, scene);

        // Asigna un material a la esfera
        var material = new BABYLON.StandardMaterial("material", scene);
        material.diffuseColor = new BABYLON.Color3(0.5, 0.5, 1);
        sphere.material = material;

        return scene;
    };

    // Crea la escena
    var scene = createScene();

    // Inicia la animación del motor de Babylon.js
    engine.runRenderLoop(function () {
        scene.render();
    });

    // Maneja el redimensionamiento de la ventana
    window.addEventListener('resize', function () {
        engine.resize();
    });
});
