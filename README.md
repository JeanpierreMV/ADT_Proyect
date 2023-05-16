# ADT_Proyect
El proyecto fue generado con dotnet new mvc --auth individual
AZURE{
en este caso mis modelos 3D se alojan en azure blobs storage(debe estar en acceso publico en este caso el acceso es de lectura anónima para contenedores y blobs)
en el uso compartido de recursos en 
Orígenes permitidos: https://localhost:x
Métodos permitidos: GET, HEAD
Encabezados permitidos:Authorization,x-ms-version,x-ms-blob-type
}
se esta utilizando los cdn d babyloin para poder vizualizar nuestro modelo 3D
_layout.cshtml
<head>  
 <script src="https://cdn.babylonjs.com/babylon.js"></script>
 <script src="https://cdn.babylonjs.com/babylon.max.js"></script>
 <script src="https://cdn.babylonjs.com/loaders/babylonjs.loaders.min.js"></script>
</head>
</head>

Controller
public IActionResult Index(){
       string urlModelo3D = "https://azustore07.blob.core.windows.net/modelo3d/index.html";
    ViewBag.UrlModelo3D = urlModelo3D;
    
    return View();
}
program

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("https://localhost:7283")
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});
app.UseCors();