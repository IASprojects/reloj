---
name: Nocturne Utility
colors:
  surface: '#121414'
  surface-dim: '#121414'
  surface-bright: '#383939'
  surface-container-lowest: '#0d0e0f'
  surface-container-low: '#1b1c1c'
  surface-container: '#1f2020'
  surface-container-high: '#292a2a'
  surface-container-highest: '#343535'
  on-surface: '#e3e2e2'
  on-surface-variant: '#c4c7c7'
  inverse-surface: '#e3e2e2'
  inverse-on-surface: '#303031'
  outline: '#8e9192'
  outline-variant: '#444748'
  surface-tint: '#c8c6c5'
  primary: '#c8c6c5'
  on-primary: '#313030'
  primary-container: '#121212'
  on-primary-container: '#7e7d7d'
  inverse-primary: '#5f5e5e'
  secondary: '#c6c6c7'
  on-secondary: '#2f3131'
  secondary-container: '#454747'
  on-secondary-container: '#b4b5b5'
  tertiary: '#c8c6c5'
  on-tertiary: '#303030'
  tertiary-container: '#121212'
  on-tertiary-container: '#7e7d7d'
  error: '#ffb4ab'
  on-error: '#690005'
  error-container: '#93000a'
  on-error-container: '#ffdad6'
  primary-fixed: '#e5e2e1'
  primary-fixed-dim: '#c8c6c5'
  on-primary-fixed: '#1c1b1b'
  on-primary-fixed-variant: '#474646'
  secondary-fixed: '#e2e2e2'
  secondary-fixed-dim: '#c6c6c7'
  on-secondary-fixed: '#1a1c1c'
  on-secondary-fixed-variant: '#454747'
  tertiary-fixed: '#e4e2e1'
  tertiary-fixed-dim: '#c8c6c5'
  on-tertiary-fixed: '#1b1c1c'
  on-tertiary-fixed-variant: '#474746'
  background: '#121414'
  on-background: '#e3e2e2'
  surface-variant: '#343535'
typography:
  display-clock:
    fontFamily: Space Mono
    fontSize: 64px
    fontWeight: '700'
    lineHeight: '1'
    letterSpacing: -0.05em
  headline-lg:
    fontFamily: Space Mono
    fontSize: 32px
    fontWeight: '700'
    lineHeight: 40px
  body-md:
    fontFamily: Geist
    fontSize: 14px
    fontWeight: '400'
    lineHeight: 20px
  label-caps:
    fontFamily: JetBrains Mono
    fontSize: 11px
    fontWeight: '600'
    lineHeight: 16px
    letterSpacing: 0.1em
  headline-lg-mobile:
    fontFamily: Space Mono
    fontSize: 24px
    fontWeight: '700'
    lineHeight: 32px
rounded:
  sm: 0.125rem
  DEFAULT: 0.25rem
  md: 0.375rem
  lg: 0.5rem
  xl: 0.75rem
  full: 9999px
spacing:
  unit: 4px
  card-padding: 1.5rem
  gutter: 1rem
  app-margin: 2rem
  stack-gap: 0.5rem
---

## Brand & Style
The design system is centered on the concept of "Digital Tactility"—merging the mechanical precision of mid-century flip clocks with a modern, high-end desktop utility aesthetic. It is designed for focused environments where the UI acts as a permanent, high-legibility fixture on the desktop.

The style is a hybrid of **Minimalism** and **Tactile Skeuomorphism**. Surfaces are treated as physical cards with subtle horizontal "split" lines to evoke flip-clock blades. The atmosphere is sober and professional, punctuated by high-energy neon accents that denote activity and focus. The emotional response should be one of "calm precision."

## Colors
The palette is dominated by "Obsidian" depths. 
- **Primary (#121212):** The main background of the application, designed to disappear into the desktop.
- **Secondary (#F5F5F5):** Used exclusively for high-contrast glyphs and numbers on cards.
- **Tertiary (#2A2A2A):** The "Card" surface color, slightly elevated from the background.
- **Neon Accents:** These are functional tokens used for the customizable border glow. They should be applied with high saturation and accompanied by a Gaussian blur (10px-20px) to simulate a light-emissive effect.

## Typography
Typography reflects the "Utility" nature of the design system. 
- **Space Mono** is used for primary data display (time, metrics, headers) to ensure every character occupies the same horizontal space, preventing UI "jitter" during value updates.
- **Geist** provides a clean, neutral sans-serif for descriptive text and settings.
- **JetBrains Mono** is used for small labels and metadata, reinforcing the technical, developer-grade precision of the tool.

## Layout & Spacing
The layout follows a **Fixed Grid** model optimized for small-footprint "widgets" or "panels" that float on the desktop.

- **The Card Unit:** All content must be housed in modular cards.
- **Spacing Rhythm:** Uses a strict 4px base unit. 
- **Desktop Strategy:** Components are compact. Navigation is typically vertical or icon-based to preserve horizontal space for data.
- **Margins:** High inner padding (24px) within cards ensures that the "Neon Glow" borders do not visually crowd the internal content.

## Elevation & Depth
Depth is achieved through **Tonal Layering** rather than traditional shadows. 
- **Level 0:** Desktop Background (Transparent/Primary).
- **Level 1:** Card Surface (#2A2A2A). Each card features a 1px solid border (#3A3A3A).
- **Neon Glow Border:** This is a secondary border placed outside the card stroke. It uses a 1.5px stroke width of the chosen Neon token, with an outer glow (box-shadow) of the same color at 15% opacity.
- **The Flip Detail:** A horizontal 1px line (#121212) should bisect the center of large numerical cards to simulate the mechanical split of a flip clock.

## Shapes
The shape language is "Soft-Industrial." Elements use a subtle 4px radius (`0.25rem`) to take the edge off the brutalist colors without appearing playful. This provides a precision-milled look, similar to high-end hardware.

## Components
- **Flip Cards:** The hero component. Deep charcoal background, off-white monospaced text. Bisected by a horizontal line.
- **Neon Toggle:** A compact switch that, when active, illuminates the card's outer border with the selected neon token.
- **Action Buttons:** Ghost buttons with 1px borders. Upon hover, the border takes on the neon accent color.
- **Input Fields:** Inset/sunken appearance. Background is slightly darker than the card surface (#1A1A1A).
- **Status Chips:** Small, monospaced text labels. The "active" state is indicated by a 4px neon dot next to the label.
- **Window Controls:** Integrated into the top-right of the primary card, minimal 12px icons for pinning/closing.