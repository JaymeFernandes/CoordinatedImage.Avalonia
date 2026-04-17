using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ImagaLoader.Demo.ViewModels;

public class MainWindowViewModel : ObservableObject
{
    public ObservableCollection<string> Paths { get; set; } = new()
    {
        "https://shared.akamai.steamstatic.com/store_item_assets/steam/spotlights/0541371913814be831bff025/spotlight_image_english.png?t=1772479050",
        "https://shared.akamai.steamstatic.com/store_item_assets/steam/spotlights/c4667407930d67132d7ae6d4/66a579b7e85ee5085031c274434355c249d60cec/spotlight_image_english.jpg?t=1775845499",
        "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/431960/header_292x136.jpg?t=1739211362",
        "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/1190000/header_292x136.jpg?t=1776275213",
        "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/4015530/d3e745805ae5013a9cf867e29925d807450b2ead/header_292x136.jpg?t=1771245843",
        "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/3976500/b8e93590d029a6eeb0c9800a23010cadfc17516b/capsule_184x69_alt_assets_0.jpg?t=1776246484",
        "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/491540/1bdc99817ca50f2cc2b442ebb9179614fa739b7e/capsule_184x69.jpg?t=1774530139",
    };
}