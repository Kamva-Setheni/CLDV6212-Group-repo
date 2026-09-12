using CoffeeAndChill.DTOs;
using CoffeeAndChill.Models;

namespace CoffeeAndChill.Interfaces;

public interface ITableStorageService
{
    Task<MenuItems> CreateMenuItemAsync(CreateMenuItemRequest request);
    Task<List<MenuItems>> GetAllMenuItemsAsync();
    Task<List<MenuItems>> GetMenuItemsByCategoryAsync(string category);
    Task<MenuItems?> GetMenuItemAsync(string category, string sku);
    Task<MenuItems?> UpdateMenuItemAsync(string category, string sku, UpdateMenuItemRequest request);
    Task<bool> DeleteMenuItemAsync(string category, string sku);
}
