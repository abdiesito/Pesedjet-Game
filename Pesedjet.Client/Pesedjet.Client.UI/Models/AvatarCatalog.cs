using System.Collections.Generic;

namespace Pesedjet.Client.UI.Models;

public static class AvatarCatalog
{
    public static IReadOnlyList<AvatarOption> All { get; } =
    [
        new(0, "M 16,12 A 4,4 0 1 1 8,12 A 4,4 0 1 1 16,12 Z M 12,3 V 5.5 M 12,18.5 V 21 M 3,12 H 5.5 M 18.5,12 H 21 " +
               "M 5.6,5.6 L 7.4,7.4 M 16.6,16.6 L 18.4,18.4 M 5.6,18.4 L 7.4,16.6 M 16.6,7.4 L 18.4,5.6"),
        new(1, "M 4,18 C 4,7 20,7 20,18 M 12,12 V 18"),
        new(2, "M 4,18 L 12,6 L 20,18 Z M 12,13 V 13.5"),
        new(3, "M 5,18 H 19 M 8,14 H 16 M 10,10 H 14 M 12,6 V 7"),
        new(4, "M 4,15 C 8,8 16,8 20,15 M 12,15 V 15.5"),
        new(5, "M 9,5 V 19 M 15,5 V 19 M 9,8 H 15 M 9,12 H 15 M 9,16 H 15"),
        new(6, "M 15.5,8.5 A 3.5,4.5 0 1 1 8.5,8.5 A 3.5,4.5 0 1 1 15.5,8.5 Z M 12,13 V 20 M 8,16 H 16"),
        new(7, "M 12,6 V 20 M 9,6 H 15 M 10,3 H 14"),
        new(8, "M 3,12 C 6,7 18,7 21,12 C 18,17 6,17 3,12 Z " +
               "M 14.5,12 A 2.5,2.5 0 1 1 9.5,12 A 2.5,2.5 0 1 1 14.5,12 Z"),
        new(9, "M 14,7 A 2,2 0 1 1 10,7 A 2,2 0 1 1 14,7 Z M 8,20 V 13 C 8,10.5 16,10.5 16,13 V 20 Z")
    ];

    public static AvatarOption? Find(int? id)
    {
        AvatarOption? found = null;

        foreach (AvatarOption avatar in All)
        {
            if (avatar.Id == id)
            {
                found = avatar;
            }
        }

        return found;
    }
}
