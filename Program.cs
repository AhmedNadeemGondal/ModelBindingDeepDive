using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
//using WebApp.Models;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapGet("/", async (HttpContext context) =>
    {
        await context.Response.WriteAsync("Welcome to the home page.");
    });

    // The following code implements custom binding using the Person class
    // The person calss has a BuildAsync mehtod that takes over and binds
    // the parameter to the source defined in the class
    endpoints.MapGet("/people", (Person? p) =>
    {
        return $"Id is {p?.Id}; Name is {p?.Name}";
    });

    //endpoints.MapGet("/employees", async (HttpContext context) =>
    //{
    //    // Get all of the employees' information
    //    var employees = EmployeesRepository.GetEmployees();

    //    context.Response.ContentType = "text/html";
    //    await context.Response.WriteAsync("<h2>Employees</h2>");
    //    await context.Response.WriteAsync("<ul>");
    //    foreach (var employee in employees)
    //    {
    //        await context.Response.WriteAsync($"<li><b>{employee.Name}</b>: {employee.Position}</li>");
    //    }
    //    await context.Response.WriteAsync("</ul>");

    //});
    ////endpoints.MapGet("/employees/{id:int}", (int id) => // This is implicit binding
    ////endpoints.MapGet("/employees/{id:int}", ([FromRoute]int id) => // This is explicit binding to the route using the parameter name
    //endpoints.MapGet("/employees/{id:int}", ([FromRoute (Name = "id")]int identityNum) => // This is explicit binding to the route using an explicit name
    ////endpoints.MapGet("/employees/{id:int}", ([FromRoute (Name = "id")] DateTime identityNum) => // Type mismatch will raise a 400 error, types and options should match exactly
    //{
    //    //return identityNum; // coe for 400 error code type mistach reproduction in the route parameter and endpoint model binding
    //    // 
    //    var employee = EmployeesRepository.GetEmployeeById(identityNum);

    //    return employee;
    //});

    // The above two endpoints are commented so the one below works

    //endpoints.MapGet("/employees",  (int id) => // this template will bind a query ?id=2 to the the endpoint handler parameter int id
    //endpoints.MapGet("/employees",  ([FromQuery(Name = "id")] int? identityNum) => // this is now explicit binding using the decorator
    // It's usually best practice to have an optional parameters when working with query strings.
    //{
    //    if (identityNum.HasValue)
    //    {
    //        var employee = EmployeesRepository.GetEmployeeById(identityNum.Value);

    //        return employee;
    //    }
    //    return null;

    //});


    //Bind from http header
    //endpoints.MapGet("/employees", ([FromHeader] int id) => // Header binding always has to be explicit
    //endpoints.MapGet("/employees/{id:int}", (int id, [FromQuery] string name, [FromHeader] string position) => // We can use multiple bindings
    endpoints.MapGet("/employees/{id:int}", ([AsParameters] GetEmployeeParameters param) => // We can use multiple bindings with the help of a class
    // struct or something similar, in this case a GetEmployeeParameters class.
    // This does not make sense but is done for demonstration.
    {
        var employee = EmployeesRepository.GetEmployeeById(param.Id);
        if (employee != null)
        {
            employee.Name = param.Name;
            employee.Position = param.Position;
        }
        return employee;
    });

    //endpoints.MapGet("/employees", ([FromQuery(Name ="id")]int[] ids) => // using query of the form ?id=1&id=2 we can send an array from the client
    endpoints.MapGet("/employees", ([FromHeader(Name = "id")] int[] ids) => // using header with multiple ids we can send an array from the client
    {
        var employees = EmployeesRepository.GetEmployees();
        var emps = employees.Where(x => ids.Contains(x.Id)).ToList();
        return emps;
    });

    // The most important things to note here are
    // 1. We are not explicitly parsing the json using a serializer.
    // 2. The framework looks at the class of the parameter and inferes the data it will bind to from the JSON body.
    // 3. It then gets the data, parses it and binds the parameter to it.
    // 4. Class types and Json keys should match exactly.
    // For minimal API the body must be JSON, this is different from MVC and Razor pages
    // Only one complex type is supported
    endpoints.MapPost("/employees", (Employee employee) =>
        {
            // The following steps should be handled by the framework
            //using var reader = new StreamReader(context.Request.Body);
            //var body = await reader.ReadToEndAsync();

            if (employee is null || employee.Id <= 0)
            {
                return "Employee is not valid";
            }

            EmployeesRepository.AddEmployee(employee);

            return "Employee added successfully";
        });

    endpoints.MapPut("/employees", async (HttpContext context) =>
    {
        using var reader = new StreamReader(context.Request.Body);
        var body = await reader.ReadToEndAsync();
        var employee = JsonSerializer.Deserialize<Employee>(body);

        var result = EmployeesRepository.UpdateEmployee(employee);
        if (result)
        {
            context.Response.StatusCode = 204;
            //await context.Response.WriteAsync("Employee updated successfully.");
            return;
        }
        else
        {
            await context.Response.WriteAsync("Employee not found.");
        }
    });

    endpoints.MapDelete("/employees/{id}", async (HttpContext context) =>
    {

        var id = context.Request.RouteValues["id"];
        var employeeId = int.Parse(id.ToString());

        if (context.Request.Headers["Authorization"] == "frank")
        {
            var result = EmployeesRepository.DeleteEmployee(employeeId);

            if (result)
            {
                await context.Response.WriteAsync("Employee is deleted successfully.");
            }
            else
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync("Employee not found.");
            }
        }
        else
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("You are not authorized to delete.");
        }

    });

});

app.Run();


class Person
{
    public int Id { get; set; }
    public string? Name { get; set; }

    public static ValueTask<Person?> BindAsync(HttpContext context)
    {
        var idString = context.Request.Query["id"];
        var nameString = context.Request.Query["name"];

        if(int.TryParse(idString, out var id))
        {
            return new ValueTask<Person?>(new Person { Id =id, Name = nameString  });
        }
        return new ValueTask<Person?>(Task.FromResult<Person?>(null));
    }
}


class GetEmployeeParameters
{
    [FromRoute]
    public int Id { get; set; }

    [FromQuery]
    public string Name { get; set; }

    [FromHeader]
    public string Position { get; set; }
}

public static class EmployeesRepository
{
    private static List<Employee> employees = new List<Employee>
{
    new Employee(1, "John Doe", "Engineer", 60000),
    new Employee(2, "Jane Smith", "Manager", 75000),
    new Employee(3, "Sam Brown", "Technician", 50000)
};

    public static List<Employee> GetEmployees() => employees;

    public static Employee? GetEmployeeById(int id)
    {
        return employees.FirstOrDefault(x => x.Id == id);
    }

    public static void AddEmployee(Employee? employee)
    {
        if (employee is not null)
        {
            employees.Add(employee);
        }
    }

    public static bool UpdateEmployee(Employee? employee)
    {
        if (employee is not null)
        {
            var emp = employees.FirstOrDefault(x => x.Id == employee.Id);
            if (emp is not null)
            {
                emp.Name = employee.Name;
                emp.Position = employee.Position;
                emp.Salary = employee.Salary;

                return true;
            }
        }

        return false;
    }

    public static bool DeleteEmployee(int id)
    {
        var employee = employees.FirstOrDefault(x => x.Id == id);
        if (employee is not null)
        {
            employees.Remove(employee);
            return true;
        }

        return false;
    }
}

public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Position { get; set; }
    public double Salary { get; set; }

    public Employee(int id, string name, string position, double salary)
    {
        Id = id;
        Name = name;
        Position = position;
        Salary = salary;
    }
}






