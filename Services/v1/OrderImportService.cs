using System.Globalization;
using CsvHelper;
using Grpc.Core;
using GrpcCrudExample.Models;
using GrpcCrudExample.v1;
using Microsoft.AspNetCore.Authorization;
using Serilog;

namespace GrpcCrudExample.Services.v1;

[Authorize]
public class OrderImportService : OrderImporter.OrderImporterBase
{
    private readonly ILogger<OrderImportService> _logger;

    public OrderImportService(ILogger<OrderImportService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Import orders from a CSV stream (streaming version)
    /// </summary>
    /// <param name="requestStream"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<ImportOrderResponse> ImportOrderCsvStream(IAsyncStreamReader<ImportOrderRequest> requestStream, ServerCallContext context)
    {
        if (requestStream == null)
            throw new ArgumentException("requestStream cannot be null");

        var orders = new List<Order>();

        await foreach (var request in requestStream.ReadAllAsync())
        {
            if (request == null)
                throw new ArgumentException("request cannot be null");

            if (string.IsNullOrEmpty(request.CsvContent))
                throw new ArgumentException("CsvContent cannot be null");

            using var reader = new StringReader(request.CsvContent);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            orders.AddRange(csv.GetRecords<Order>());
        }

        _logger.LogInformation("Imported {Count} orders", orders.Count);

        // Here you would typically save the orders to a database
        // For this example, we'll just return the count

        return new ImportOrderResponse { ImportedCount = orders.Count };

    }

    /// <summary>
    /// Import orders from a CSV string
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override Task<ImportOrderResponse> ImportOrderCsv(ImportOrderRequest request, ServerCallContext context)
    {
        if (request == null)
            throw new ArgumentException("request cannot be null"); ;

        if (string.IsNullOrEmpty(request.CsvContent))
            throw new ArgumentException("CsvContent cannot be null");

        var orders = new List<Order>();

        using var reader = new StringReader(request.CsvContent);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        orders.AddRange(csv.GetRecords<Order>());

        _logger.LogInformation("Imported {Count} orders (non-streaming)", orders.Count);

        // Here you would typically save the orders to a database
        // For this example, we'll just return the count

        return Task.FromResult(new ImportOrderResponse { ImportedCount = orders.Count });
    }

    /// <summary>
    /// Export orders to a CSV file
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override Task<ExportOrderResponse> ExportOrderCsv(ExportOrderRequest request, ServerCallContext context)
    {
        // For this example, we'll create some dummy orders
        var orders = new List<Order>
        {
            new Order { OrderId = 1, OrderDate = DateTime.Now, CustomerName = "John Doe", ProductName = "Widget", Quantity = 5, Price = 9.99m },
            new Order { OrderId = 2, OrderDate = DateTime.Now.AddDays(-1), CustomerName = "Jane Smith", ProductName = "Gadget", Quantity = 2, Price = 24.99m }
        };

        using var writer = new StringWriter();
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        csv.WriteRecords(orders);

        var csvContent = writer.ToString();

        _logger.LogInformation("Exported {Count} orders", orders.Count);

        return Task.FromResult(new ExportOrderResponse { CsvContent = csvContent });
    }

    /// <summary>
    /// Export orders to a CSV file (streaming version)
    /// </summary>
    /// <param name="request"></param>
    /// <param name="responseStream"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task ExportOrderCsvStream(ExportOrderRequest request, IServerStreamWriter<ExportOrderResponse> responseStream, ServerCallContext context)
    {
        // For this example, we'll create some dummy orders
        var orders = new List<Order>
        {
            new Order { OrderId = 1, OrderDate = DateTime.Now, CustomerName = "John Doe", ProductName = "Widget", Quantity = 5, Price = 9.99m },
            new Order { OrderId = 2, OrderDate = DateTime.Now.AddDays(-1), CustomerName = "Jane Smith", ProductName = "Gadget", Quantity = 2, Price = 24.99m }
        };

        using var writer = new StringWriter();
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        csv.WriteHeader<Order>();
        await csv.NextRecordAsync();

        foreach (var order in orders)
        {
            csv.WriteRecord(order);
            await csv.NextRecordAsync();

            var csvContent = writer.ToString();
            writer.GetStringBuilder().Clear();

            await responseStream.WriteAsync(new ExportOrderResponse { CsvContent = csvContent });
        }

        _logger.LogInformation("Exported {Count} orders (streaming)", orders.Count);
    }
}

