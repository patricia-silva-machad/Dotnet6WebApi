//criar a aplicacao web (hosting)
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

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


app.Run();

public static class ProductRepository {
    public static List<Product> Products { get; set; }

    public static void Add(Product product){
        if(Products == null)
            Products = new List<Product>();
        Products.Add(product);
    }

    public static Product GetBy(string code){
        return Products.FirstOrDefault(p => p.Code == code);
    }

    public static void Remove(Product product) {
        Products.Remove(product);
    }
}


public class Product {

    public String Code { get; set; }

    public String Name { get; set; }
}
