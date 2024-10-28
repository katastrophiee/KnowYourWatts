using System.ComponentModel.DataAnnotations;

namespace KnowYourWatts.ClientUI.DTO.Enums;

public enum TariffType
{
    [Display(Name = "Fixed")]
    Fixed = 0,

    [Display(Name = "Flex")]
    Flex = 1,

    [Display(Name = "Green")]
    Green = 2,

    [Display(Name = "OffPeak")]
    OffPeak = 3
}
