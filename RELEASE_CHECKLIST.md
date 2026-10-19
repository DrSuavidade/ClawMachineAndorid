# Claw Machine Android - Release Checklist

Use this checklist to verify that a build is ready for release (e.g., to Google Play Store). Check each item and mark as completed.

## Build Settings
- [ ] **Scripting Backend:** Set to IL2CPP
- [ ] **Target Architectures:** ARM64 required (ARMv7 optional; x86 excluded for Play Store compliance)
- [ ] **Orientation:** Locked to Portrait
- [ ] **Minimum API Level:** Set appropriately (e.g., API 21 Android 5.0 or higher; determined by Unity compatibility and intended supported devices)
- [ ] **Target SDK (API Level):** Set to API 36 (required for new standard Android app submissions in October 2026)
- [ ] **Target Frame Rate:** Set to 60 fps (or player preference respected)
- [ ] **Texture Compression:** ASTC or ETC2 enabled for Android
- [ ] **Strip Engine Code:** Enabled (to reduce build size)
- [ ] **Managed Stripping Level:** Medium or High (with necessary link.xml preserves)
- [ ] **Debugging Symbols:** Set to appropriate level (e.g., Public for crash symbolication)
- [ ] **Build System:** Gradle (Internal or Custom) selected
- [ ] **Custom Main Gradle Template:** If used, verified compatible
- [ ] **Launcher Icons:** Adaptive icons provided for all required densities
- [ ] **Splash Screen:** Configured (if using Unity 2021.2+)
- [ ] **Android App Bundle:** Building as .aab (recommended for Play Store)
- [ ] **Version Code and Name:** Incremented from previous build, following semantic versioning
- [ ] **Package Identifier:** Matches the one registered in Play Console (`com.drsuavidade.clawmachine`)
- [ ] **App Name:** Matches store listing
- [ ] **Icon High Resolution:** 1024x1024 PNG provided for store listing

## Gameplay & Content
- [ ] **Core Gameplay Loop:** Claw aim, drop, grip, lift, carry, return, release functions correctly
- [ ] **Auto-Return Sequence:** After lift, claw automatically travels to chute, drops prize, returns home
- [ ] **Dynamic Grip System:** Clean grip, tension slip, near-miss nudge mechanics working and balanced
- [ ] **Input System:** Virtual joystick provides smooth, continuous control; no accidental screen drags
- [ ] **Modal Blocking:** Inputs blocked while any modal dialog is open
- [ ] **Machine Swiping:** Restricted to header buttons or deliberate top-bar swipe
- [ ] **Economy:** Coin awards balanced; passive income only from owned machines; set bonus applied correctly
- [ ] **Duplicate Prize Handling:** Duplicates stored in inventory, coins awarded only on sale
- [ ] **Offline Earnings:** Calculated correctly based on owned machines and elapsed time
- [ ] **Prize Spawning:** Distribution approximately Normal 70% / Rare 25% / Secret 5%
- [ ] **Reward System:** Rewarded ad prompt appears when out of coins; grants promised free plays
- [ ] **Dual Currency:** Gems/tickets earned and spendable (if cosmetic store implemented)
- [ ] **Collection UI:** Displays discovered prizes, allows selling duplicates, shows set completion bonuses
- [ ] **Progression:** Machines unlock based on achievement; owned machines provide passive income and set bonus
- [ ] **Win Conditions:** Prize successfully dropped into chute counts as win; near-miss nudges can cause cascade wins
- [ ] **Audio:** Real audio clips used (motor hum, claw clank, grab chord, chute drop, win fanfare); background music optional with volume control
- [ ] **Haptics:** Short, crisp vibration feedback for claw actions (no prolonged vibration)
- [ ] **Particle Effects:** Lift tension sparks, cable strain, dust puff on ground touch, impact burst on prize nudge, confetti on win, sparkle on rare/secret prizes
- [ ] **UI:** All text uses TextMeshPro; no legacy Unity Text components
- [ ] **UI Consistency:** Rarity palette applied (Normal=Cyan, Rare=Purple, Secret=Gold) across all UI elements
- [ ] **UI Responsiveness:** UI scales correctly across different screen resolutions and aspect ratios
- [ ] **No Placeholder Content:** All temporary/prototype assets replaced (e.g., placeholder audio, primitive prize models if 3D models are intended)
- [ ] **No Debug Overlays:** No IMGUI debug boxes, no visible debug logs in release build
- [ ] **Accessibility:** Basic accessibility considerations (color contrast, touch target size) reviewed

## Technical & Performance
- [ ] **Project Builds Successfully:** No compilation errors; build completes without errors
- [ ] **Application Launches:** Game starts from launcher icon to main menu without crashes
- [ ] **Scene Loading:** All scenes load correctly; no missing assets or pink materials
- [ ] **Memory Usage:** Reasonable RAM usage on target devices (e.g., < 200MB on mid-range devices)
- [ ] **Frame Rate:** Consistently near target fps (55-60fps) during gameplay; no major drops
- [ ] **Battery Usage:** Not excessively draining (subjective but reasonable)
- [ ] **Touch Input:** Accurate and responsive; no dead zones or offset
- [ ] **Audio Latency:** Sounds play in sync with actions; no noticeable delay
- [ ] **Network Requests:** If any (e.g., ads, analytics), they complete successfully and handle errors gracefully
- [ ] **Save/Load System:** Game saves and loads correctly; progress persists across sessions
- [ ] **Clear Data Option:** Ability to clear player data (for testing) works correctly
- [ ] **No Exceptions in Log:** adb logcat shows no unhandled exceptions or spammy warnings
- [ ] **Android Vitals:** No excessive crash rate, ANR rate, or stuck wake locks (if available from internal testing)

## Legal & Compliance
- [ ] **Privacy Policy:** Link provided in store listing and accessible within game (if required)
- [ ] **Terms of Service:** Link provided if applicable
- [ ] **Ads Implementation:** If using ads, comply with relevant policies (e.g., Google AdMob, Unity LevelPlay); rewarded ads are voluntary and clearly labeled
- [ ] **In-App Purchases:** If applicable, implemented correctly and comply with store policies
- [ ] **Content Rating:** Appropriate rating assigned (e.g., Everyone for casual arcade game)
- [ ] **Copyright:** All assets (audio, models, sprites) are either original, licensed, or covered under appropriate licenses; no infringing content
- [ ] **Third-Party Licenses:** All third-party libraries and SDKs comply with their licenses; attributions provided if required

## Final Verification
- [ ] **Internal QA Build:** Distributed to internal testers; feedback addressed
- [ ] **Device Test Matrix:** Tested on a representative set of devices (various screen sizes, hardware capabilities, Android versions)
- [ ] **Play Store Listing:** Screenshots, feature graphic, video, and description uploaded and accurate
- [ ] **Release Notes:** Prepared for the version update
- [ ] **Rollback Plan:** Known good version available in case of critical post-release issue

## Sign-Off
- [ ] **Release Engineer:** ______________________   Date: __________
- [ ] **QA Lead:** ____________________________   Date: __________
- [ ] **Product Owner:** _______________________   Date: __________

