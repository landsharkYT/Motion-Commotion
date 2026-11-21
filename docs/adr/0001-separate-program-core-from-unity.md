# Separate the program core from Unity presentation and physics

Formula evaluation, program validation, and command scheduling use plain C# without Unity dependencies. Unity components own presentation and physics, with one session connecting the parts. This keeps the future level editor independent of the block UI and makes timing and mathematical rules verifiable without loading a scene, at the cost of explicit data and action boundaries.

Formula evaluation has no side effects. Executing a weight command changes gravity, while evaluating a nested weight formula only produces a value. Assembly definitions enforce the core's independence. Mutable program and experiment state belong to a session, separate from shared definition assets.
