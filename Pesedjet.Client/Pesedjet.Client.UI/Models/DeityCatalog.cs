using System.Collections.Generic;

namespace Pesedjet.Client.UI.Models;

public static class DeityCatalog
{
    public const int AtumRaValue = 9;
    public const int NeftisValue = 1;
    public const int MinimumValue = 0;
    public const int MaximumValue = 9;

    public static IReadOnlyList<DeityCard> All { get; } =
    [
        new(9, "Atum-Ra", CardMetal.Silver, 1, "M 12,8 A 4,4 0 1 1 12,16 A 4,4 0 1 1 12,8 Z " +
            "M 12,2 V 5 M 12,19 V 22 M 2,12 H 5 M 19,12 H 22", false, false),
        new(8, "Shu", CardMetal.Gold, 1, "M 4,16 C 8,8 16,8 20,16 M 12,10 V 20", false, false),
        new(7, "Tefnut", CardMetal.Gold, 1, "M 5,18 C 5,10 19,10 19,18 M 8,18 V 21 M 16,18 V 21 " +
            "M 9,12 L 12,6 L 15,12", true, false),
        new(6, "Geb", CardMetal.Iron, 2, "M 3,19 H 21 M 5,19 L 9,10 L 13,16 L 16,12 L 19,19", false, false),
        new(5, "Nut", CardMetal.Iron, 2, "M 3,9 C 7,3 17,3 21,9 M 6,9 V 20 M 18,9 V 20", true, false),
        new(4, "Osiris", CardMetal.Iron, 2, "M 12,3 V 21 M 8,7 H 16 M 9,12 H 15 M 10,3 H 14", false, false),
        new(3, "Isis", CardMetal.Bronze, 2, "M 9,4 V 20 M 15,4 V 20 M 9,8 H 15 M 9,12 H 15 M 9,16 H 15", true, false),
        new(2, "Seth", CardMetal.Copper, 2, "M 4,16 C 8,6 14,6 18,10 L 20,8 M 12,10 V 20", true, false),
        new(1, "Neftis", CardMetal.Copper, 6, "M 3,12 C 6,7 18,7 21,12 C 18,17 6,17 3,12 Z " +
            "M 14.5,12 A 2.5,2.5 0 1 1 9.5,12 A 2.5,2.5 0 1 1 14.5,12 Z", true, true),
        new(0, "Ushabti", CardMetal.Clay, 2, "M 12,4 A 3,3 0 1 1 12,10 A 3,3 0 1 1 12,4 Z " +
            "M 8,20 V 13 C 8,11 16,11 16,13 V 20 Z", false, false)
    ];

    public static DeityCard Find(int value)
    {
        DeityCard found = All[0];

        foreach (DeityCard card in All)
        {
            if (card.Value == value)
            {
                found = card;
            }
        }

        return found;
    }
}
