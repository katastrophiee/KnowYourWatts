using System.ComponentModel.DataAnnotations;

namespace KnowYourWatts.DTO.Enums;

public enum TarrifType
{
    [Display(Name = "Fixed")]
    Fixed = 0,

    [Display(Name = "Fixed")]
    Flex = 1,

    [Display(Name = "Fixed")]
    Green = 2,

    [Display(Name = "Fixed")]
    OffPeak = 3
}
