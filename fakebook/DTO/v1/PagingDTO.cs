using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace fakebook.DTO.v1;

public class PagingDTO
{
    [DefaultValue(0)]
    public int PageIndex { get; set; } = 0;
    
    [DefaultValue(10)]
    [Range(1, 100)]
    public int PageSize { get; set;} = 10;
    
    [DefaultValue(null)]
    public string? Q { get; set; } = null;
}
