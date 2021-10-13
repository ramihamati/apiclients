using System.ComponentModel.DataAnnotations;

namespace ComX.Common.ApiClients.Tests.Models
{
    public enum Colors
    {
        Blue = 0,
        [Display(Name = "Rosu")]
        Red = 1
    }

}
