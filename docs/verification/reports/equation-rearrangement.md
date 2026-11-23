# Equation rearrangement verification

Contract: [approved rearrangement scope and rules](../../design/equation-rearrangement.md#consolidated-approved-contract).

The implementation adds 13 curated variants to the five original forms. Palette-only pencils open a symbolic editor with inverse-operation menus, Swap, Undo/Reset and explicit time-branch selection. Confirmation adds a reusable template for the current level visit. Time and initial-condition outputs are pure value blocks; mass, gravity, displacement and velocity-change variants have the approved physical actions.

## Native coverage

The tests exercise formula values, chronological roots for both acceleration signs, repeated/zero/negative/no-real roots, linear and stationary degeneracies, time and mass limits, other zero denominators, cancellation and coefficient underflow. Full motion-term paths reach initial position, initial velocity and acceleration, including both coefficient/factor orders. Ordered compilation carries mass forward and rejects invalid later expressions before any physical action.

PlayMode checks cover creation through actual editor controls, operation enablement, intermediate confirmation blocking, swaps, popup cancellation, history, explicit branch selection, duplicate detection, palette-only pencils, pure shapes and all-variant right-side input bounds. They also cover pause retention, template lifetime and the new physical actions while retaining the earlier physics tests.

An early test caught confirmation incorrectly surviving a time-form side swap. Another caught a missing CanvasRenderer on the new radical graphic. Both were corrected before the final verification run.

Current native results: 85 EditMode and 68 PlayMode tests passed, 153 total. The focused rearrangement browser run passed all four interaction groups with zero application errors, including real touchscreen symbol dragging and confirmation. The new build completed successfully under job 884e9d948fd2485fad9c42294ba15eb4.

The full general browser suite has not yet been rerun against this build. Final screenshot inspection and the independent adversarial rating are pending. The previous full browser result belongs to the earlier menu implementation and must not be reported as a fresh rearrangement regression pass.

See the [continuation note](../../design/equation-rearrangement-continuation.md) for remaining work.

## Repeatable checks

```sh
python3 Tools/verify-unity.py
python3 Tools/verify-layout.py
npm --prefix Tools run verify:web
MOTION_REARRANGEMENT_ONLY=1 npm --prefix Tools run verify:web
```

Build the current Web proof through Motion Commotion > Build Web proof before browser checks. The focused mode drives symbolic editor operations with real mouse input, creates and uses level-local variants, checks branch selection and input bounds, verifies calculated mass during execution, and performs touchscreen dragging and confirmation.

The targeted prefab upgrade is Motion Commotion > Upgrade equation editor. It preserves level definitions, geometry, scene GUIDs and other modal contents. The full foundation generator includes the same editor skeleton and still seeds only the five original formulas.
