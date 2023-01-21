//criar a aplicacao web (hosting)
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

//configuração de serviços para poder utilizar os endpoints quando necessario;
builder.Services.AddDbContext<ApplicationDbContext>();


var app = builder.Build();
var configuration = app.Configuration;
ProductRepository.Init(configuration);

app.MapGet("/", () => "Hello World 4!");
app.MapGet("/user", () => new {Name = "Patricia", Age = 0});
app.MapGet("/AddHeader", (HttpResponse response) => {
    response.Headers.Add("Teste", "Patricia Machado");
    return new {Name = "Patricia", Age = 0};
    });

app.MapPost("/products", (Product product) => {
    ProductRepository.Add(product); //interpolação da string linha 16 apos $ 
    return Results.Created($"/products/{product.Code}" , product.Code);
});
//consulta, passando por parametros (tudo que estiver depois do ?) via query
//api.app.com/users?datastart={date}&dateend={date}
app.MapGet("/getProduct", ([FromQuery] string dateStart, [FromQuery] string dateEnd) =>{
    return dateStart + " - " + dateEnd;
});
//atraves da rota (quando é obrigatorio)
//api.app.com/user/{code}
app.MapGet("/products/{code}", ([FromRoute] String code) => {
    var product = ProductRepository.GetBy(code);
    if(product != null)
        return Results.Ok(product);
    return Results.NotFound();
});

//parametros pelo header (ex: quando quer enviar um token)
app.MapGet("/getproductbyheader", (HttpRequest request) => {
    return request.Headers["product-code"].ToString();
});

app.MapPut("/products", (Product product) => {
    var productSaved = ProductRepository.GetBy(product.Code);
    productSaved.Name = product.Name;
    return Results.Ok();
});

app.MapDelete("/products/{code}", ([FromRoute] String code) => {
    var productSaved = ProductRepository.GetBy(code);
    ProductRepository.Remove(productSaved);
    return Results.Ok();
});


//endpoint so vai ser encontrado se estiver no ambiente staging
if(app.Environment.IsStaging())
    app.MapGet("/configuration/database", (IConfiguration configuration) => {
        return Results.Ok($"{configuration["database:connection"]}/{configuration["database:port"]}");
    });


app.Run();

public static class ProductRepository {
    public static List<Product> Products { get; set; } = Products = new List<Product>();

    public static void Init(IConfiguration configuration) {
        var products = configuration.GetSection("Products").Get<List<Product>>();
        Products = products;
    }

    public static void Add(Product product){      
        Products.Add(product);
    }

    public static Product GetBy(string code){
        return Products.FirstOrDefault(p => p.Code == code);
    }

    public static void Remove(Product product) {
        Products.Remove(product);
    }
}

public class Category {
    public int Id { get; set; }

    public string Name { get; set; }
}

public class Product {

    public int Id { get; set; }
    public String Code { get; set; }

    public String Name { get; set; }

    public string Description { get; set; }

    public int CategoryId { get; set; }

    public Category Category { get; set; }
}

public class ApplicationDbContext: DbContext{
    
    protected override void OnModelCreating(ModelBuilder builder){
        builder.Entity<Product>()
            .Property(p => p.Description).HasMaxLength(500).IsRequired(false);
        builder.Entity<Product>()
            .Property(p => p.Name).HasMaxLength(120).IsRequired();
        builder.Entity<Product>()
            .Property(p => p.Code).HasMaxLength(10).IsRequired();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    => options.UseSqlServer(
        "Server=localhost;Database=Products;User Id=sa;Password=@sql2019;MultipleActiveResultSets=true;Encrypt=YES;TrustServerCertificate=YES");
}
