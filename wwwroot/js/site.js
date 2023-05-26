 // Configura el contenedor y la escena
 var container = document.getElementById('canvas-container');
 var scene = new THREE.Scene();

 // Configura la cámara
 var camera = new THREE.PerspectiveCamera(75, window.innerWidth / window.innerHeight, 0.1, 1000);
 camera.position.z = 5;

 // Configura el renderizador
 var renderer = new THREE.WebGLRenderer();
 renderer.setSize(window.innerWidth, window.innerHeight);
 container.appendChild(renderer.domElement);

 // Carga el modelo 3D
 var loader = new THREE.OBJLoader();
 loader.load('ruta_al_archivo.obj', function (object) {
     scene.add(object);
 });

 // Renderiza la escena
 function animate() {
     requestAnimationFrame(animate);
     renderer.render(scene, camera);
 }
 animate();

