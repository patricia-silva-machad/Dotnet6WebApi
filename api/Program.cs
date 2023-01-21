//criar a aplicacao web (hosting)
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

//configuração de serviços para poder utilizar os endpoints quando necessario;
builder.Services.AddSqlServer<ApplicationDbContext>(builder.Configuration["Database:SqlServer"]);

var app = builder.Build();
var configuration = app.Configuration;
ProductRepository.Init(configuration);

app.MapGet("/", () => "Hello World 4!");
app.MapGet("/user", () => new {Name = "Patricia", Age = 0});
app.MapGet("/AddHeader", (HttpResponse response) => {
    response.Headers.Add("Teste", "Patricia Machado");
    return new {Name = "Patricia", Age = 0};
    });

app.MapPost("/products", (ProductRequest productRequest, ApplicationDbContext context) => {
    var category = context.Categories.Where(c => c.Id == productRequest.CategoryId).First();
    var product = new Product {
        Code = productRequest.Code,
        Name = productRequest.Name,
        Description = productRequest.Description,
        Category = category
    };

    if(productRequest.Tags != null) {
        product.Tags = new List<Tag>();
        foreach(var item in productRequest.Tags){
            product.Tags.Add(new Tag{ Name = item});
        }
    }
    context.Products.Add(product);
    context.SaveChanges();
    return Results.Created($"/products/{product.Id}" , product.Id); //interpolação da string linha 16 apos $ 
});
//consulta, passando por parametros (tudo que estiver depois do ?) via query
//api.app.com/users?datastart={date}&dateend={date}
app.MapGet("/getProduct", ([FromQuery] string dateStart, [FromQuery] string dateEnd) =>{
    return dateStart + " - " + dateEnd;
});
//atraves da rota (quando é obrigatorio)
//api.app.com/user/{code}
app.MapGet("/products/{id}", ([FromRoute] int id, ApplicationDbContext context) => {
    var product = context.Products
    .Include(p => p.Category)
    .Include(p => p.Tags)
    .Where(p => p.Id == id).First();
    if(product != null) {
        return Results.Ok(product);
    }
    return Results.NotFound();
});

//parametros pelo header (ex: quando quer enviar um token)
app.MapGet("/getproductbyheader", (HttpRequest request) => {
    return request.Headers["product-code"].ToString();
});

app.MapPut("/products/{id}", ([FromRoute] int id, ProductRequest productRequest, ApplicationDbContext context) => {
    var product = context.Products
        .Include(p => p.Tags)
        .Where(p => p.Id == id).First();
    var category = context.Categories.Where(c => c.Id == productRequest.CategoryId).First();

    product.Code = productRequest.Code;
    product.Name = productRequest.Name;
    product.Description = productRequest.Description;
    product.Category = category;
    product.Tags = new List<Tag>();
    //remove a tag da instancia e em seguida atualiza a lista
    if(productRequest.Tags != null) {
        product.Tags = new List<Tag>();
        foreach(var item in productRequest.Tags){
            product.Tags.Add(new Tag{ Name = item});
        }
    }
    context.SaveChanges();
    return Results.Ok();
});

app.MapDelete("/products/{id}", ([FromRoute] int id, ApplicationDbContext context) => {
    var product = context.Products.Where(p => p.Id == id).First();
    context.Products.Remove(product);
    context.SaveChanges();
    return Results.Ok();
});

//endpoint so vai ser encontrado se estiver no ambiente staging
if(app.Environment.IsStaging())
    app.MapGet("/configuration/database", (IConfiguration configuration) => {
        return Results.Ok($"{configuration["database:connection"]}/{configuration["database:port"]}");
    });

app.Run();
