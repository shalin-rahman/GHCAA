# PLAN.md: Area 8.2 - Adaptive Layout for Tablets/Pads

## Objective
Implement an adaptive layout in the Flutter mobile application to support tablets and large screen devices. For larger screens (e.g., width > 600px), replace the `BottomNavigationBar` and `Drawer` with a persistent `NavigationRail` or `Sidebar` architecture to utilize the extra horizontal space efficiently.

## Technical Approach
We will utilize Flutter's `LayoutBuilder` and `MediaQuery` to detect screen width constraints. The primary navigation container (`AppScaffold` or equivalent root scaffold) will dynamically return either a mobile-optimized or tablet-optimized layout.

## Execution Steps

- [x] **Step 1: Identify and Update Root Navigation Scaffold**
  - Locate the main scaffold wrapper used across the application (usually `app_scaffold.dart` or `dashboard_screen.dart`).
  - Wrap the main scaffold body with a `LayoutBuilder`.
  - Define a breakpoint constant (e.g., `const double kTabletBreakpoint = 600.0;`).

- [x] **Step 2: Implement NavigationRail (Tablet UI)**
  - For screens wider than `kTabletBreakpoint`, render a `Row` containing a `NavigationRail` on the left and the main screen content (`Expanded`) on the right.
  - Map the existing `BottomNavigationBarItem` properties to `NavigationRailDestination`.
  - Hide the standard `Drawer` and `BottomNavigationBar` in the tablet layout.

- [x] **Step 3: Refactor Main Screens for Horizontal Scaling**
  - Ensure the content area does not stretch infinitely (e.g., forms or text). 
  - Add `Center` + `ConstrainedBox` wrappers to specific high-density screens (like Profile, Registration, Login) if they stretch too wide on a tablet layout.

- [ ] **Step 4: Verify Adaptive Behavior**
  - Run the Flutter application on a desktop/tablet simulator.
  - Ensure state is maintained when resizing the window across the breakpoint.
  - Fix any visual overflow or render errors caused by the new layout constraints.

- [ ] **Step 5: E2E / Visual Regression Integration**
  - Check if the visual test harness requires any updates to test both mobile and tablet breakpoints. (Optional for this phase, but good practice).
