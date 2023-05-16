window.addEventListener('DOMContentLoaded', function () {
    // Crear el lienzo (canvas) de Babylon.js
    var canvas = document.getElementById('renderCanvas');
    var engine = new BABYLON.Engine(canvas, true);
  
    // Crear la escena principal
    const sceneId = "miEscena";
    var createScene = function () {
      var scene = new BABYLON.Scene(engine);
  
      // Crear una cámara
      var camera = new BABYLON.ArcRotateCamera('camera', 0, 0, 0, BABYLON.Vector3.Zero(), scene);
      camera.setPosition(new BABYLON.Vector3(0, 5, -10));
      camera.attachControl(canvas, true);
  
      // Crear una luz
      var light = new BABYLON.HemisphericLight('light', new BABYLON.Vector3(0, 1, 0), scene);
  
      // Cargar el modelo 3D desde Azure Blob Storage
      var modelUrl = '';
      BABYLON.SceneLoader.ImportMesh('', modelUrl, '', scene, function (meshes) {
        // Hacer algo con los meshes del modelo 3D cargado, si es necesario
      });
  
      return scene;
    };
  
    // Crear la escena
    var scene = createScene();
  
    // Iniciar el ciclo de renderizado de Babylon.js
    engine.runRenderLoop(function () {
      scene.render();
    });
  
    // Ajustar el tamaño del lienzo cuando la ventana del navegador cambia de tamaño
    window.addEventListener('resize', function () {
      engine.resize();
    });
  });