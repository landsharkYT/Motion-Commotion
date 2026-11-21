# Own simulation time with one controller

One controller manually advances Unity 2D physics in fixed steps and coordinates program execution against the same clock. Consecutive kicks execute at the same simulation instant unless separated by Wait, and Pause freezes both execution and physics. Manual stepping requires explicit scheduling, but avoids distributing those rules across automatic physics callbacks, UI animations, and coroutine timing.

Fixed stepping improves consistency without promising identical floating-point outcomes across platforms. Run captures the edited starting configuration. Reset restores it and the physical starting state and clock while preserving the program.
