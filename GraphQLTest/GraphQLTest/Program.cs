using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddDbContext<GraphQLDbContext>(o => o
        .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=GraphQLDbContext;Trusted_Connection=True;"));

builder.Services
    .AddGraphQLServer()
    .RegisterDbContext<GraphQLDbContext>()
    .AddQueryType<Query>()
    .AddType<Bar>()
    .AddProjections()
    .AddFiltering();

var app = builder.Build();

var serviceScopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
using (var scope = serviceScopeFactory.CreateScope())
using (var ctx = scope.ServiceProvider.GetRequiredService<GraphQLDbContext>())
{
    ctx.Database.EnsureDeleted();
    ctx.Database.EnsureCreated();
    ctx.Foos.Add(new Foo {Id = 1, X = 1});
    ctx.Foos.Add(new Bar {Id = 2, X = 2, Y = 3});
    ctx.SaveChanges();
}


app.MapGet("/", () => "Hello World!");
app.MapGraphQL();
app.Run();

public class GraphQLDbContext : DbContext
{
    protected GraphQLDbContext()
    {
    }

    public GraphQLDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Foo> Foos => Set<Foo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Foo>().ToTable("Foo");
        modelBuilder.Entity<Bar>().ToTable("Bar");
    }
    
    
}

public class Query
{
    [UseProjection]
    [UseFiltering]
    public IEnumerable<Foo> GetFoos(GraphQLDbContext dbContext) => dbContext.Foos;
}

[InterfaceType("Foo")]
public class Foo
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }
    public int X { get; set; }
}

public class Bar : Foo
{
    public int Y { get; set; }
}