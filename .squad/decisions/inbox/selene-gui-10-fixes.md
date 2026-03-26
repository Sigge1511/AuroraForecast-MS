### 2026-03-27T00:21: GUI fix decisions for Issue #10
**By:** Selene (requested by Sigge)
**What:** Three GUI fixes in `AuroraFix/Views/MainPage.xaml` for Issue #10.

#### Circle geometry
Chose `StartPoint="160,25"` / `Size="135,135"` / `Point="159.9,25"` for both Path elements in a 320×320 Grid. Arc centre = (160, 25+135) = (160,160) which equals (Width/2, Height/2). The tiny X-offset on the endpoint (159.9 vs 160) prevents MAUI from collapsing a zero-length arc. Grid was grown from 300×300 to 320×320 rather than shrinking the arc, to give text more room inside the ring. Both paths are geometrically identical so the background halo and teal arc track perfectly.

#### Inner text layout
Removed `Margin="-10,3,0,0"` from the inner `VerticalStackLayout`. The negative left margin was a legacy pixel-nudge that compensated (badly) for the mis-centred arc. With correct geometry it is no longer needed — `HorizontalOptions="Center"` and `VerticalOptions="Center"` alone are sufficient.

#### Bottom margin recalibration
Changed ring Grid `Margin` from `0,0,0,-30` to `0,0,0,-25`. In the old 300×300 grid, the circle path bottom was at y=270 and `300-270=30`, so `-30` perfectly aligned the MORE card with the circle bottom. New circle bottom is at y=295 in the 320px grid; correct overlap value is `-(320-295) = -25`.

#### Forecast dimming
Root cause was renderer-level: `CollectionView` had no explicit `SelectionMode` or background colour. Platform renderers can apply default selection/hover fills to item containers. Fix: `SelectionMode="None"` + `BackgroundColor="Transparent"` on `CollectionView`; `BackgroundColor="Transparent"` on the DataTemplate `Grid`. The intentional `Opacity="0.7"` on the secondary KP INDEX label was deliberately preserved (it is a visual hierarchy signal).

**Why:** Issue #10 resolution — three separate visual defects in the probability ring and forecast section.
**PR:** https://github.com/Sigge1511/AuroraForecast-MS/pull/12
