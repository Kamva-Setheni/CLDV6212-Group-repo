using Azure;
using Azure.Data.Tables;
using CoffeeAndChill.DTOs;
using CoffeeAndChill.Interfaces;
using CoffeeAndChill.Models;
using Microsoft.Extensions.Configuration;

namespace CoffeeAndChill.Services;

public class TableStorageService : ITableStorageService
{
    private readonly TableClient _tableClient;

    public TableStorageService(IConfiguration configuration)
    {
        var connectionString = configuration["AzureWebJobsStorage"]
            ?? throw new InvalidOperationException("AzureWebJobsStorage connection string is missing.");
        _tableClient = new TableClient(connectionString, "MenuItems");
        _tableClient.CreateIfNotExists();
    }

    public async Task<MenuItems> CreateMenuItemAsync(CreateMenuItemRequest request)
    {
        var menuItem = new MenuItems
        {
            PartitionKey = request.Category,
            RowKey = request.SKU,
            Category = request.Category,
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            IsAvailable = request.IsAvailable
        };

        try
        {
            await _tableClient.AddEntityAsync(menuItem);
            return menuItem;
        }
        catch (RequestFailedException ex) when (ex.Status == 409)
        {
            throw new InvalidOperationException(
                "A menu item with this SKU already exists in this category.", ex);
        }
    }

    public async Task<bool> DeleteMenuItemAsync(string category, string sku)
    {
        var menuItem = await GetMenuItemAsync(category, sku);
        if (menuItem is null)
        {
            return false;
        }

        try
        {
            await _tableClient.DeleteEntityAsync(category, sku, menuItem.ETag);
            return true;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return false;
        }
    }

    public async Task<List<MenuItems>> GetAllMenuItemsAsync()
    {
        var menuItems = new List<MenuItems>();
        await foreach (var item in _tableClient.QueryAsync<MenuItems>())
        {
            menuItems.Add(item);
        }

        return menuItems;
    }

    public async Task<MenuItems?> GetMenuItemAsync(string category, string sku)
    {
        try
        {
            var response = await _tableClient.GetEntityAsync<MenuItems>(category, sku);
            return response.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    public async Task<List<MenuItems>> GetMenuItemsByCategoryAsync(string category)
    {
        var menuItems = new List<MenuItems>();
        var escapedCategory = category.Replace("'", "''", StringComparison.Ordinal);
        await foreach (var item in _tableClient.QueryAsync<MenuItems>($"PartitionKey eq '{escapedCategory}'"))
        {
            menuItems.Add(item);
        }

        return menuItems;
    }

    public async Task<MenuItems?> UpdateMenuItemAsync(string category, string sku, UpdateMenuItemRequest request)
    {
        var menuItem = await GetMenuItemAsync(category, sku);
        if (menuItem is null)
        {
            return null;
        }

        menuItem.Name = request.Name;
        menuItem.Description = request.Description;
        menuItem.Price = request.Price;
        menuItem.IsAvailable = request.IsAvailable;
        await _tableClient.UpdateEntityAsync(menuItem, menuItem.ETag, TableUpdateMode.Replace);
        return menuItem;
    }
}
