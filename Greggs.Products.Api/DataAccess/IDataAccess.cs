using System.Collections.Generic;

namespace Greggs.Products.Api.DataAccess;

public interface IDataAccess<out T>
{
    IAsyncEnumerable<T> List(int? pageStart, int? pageSize);
}