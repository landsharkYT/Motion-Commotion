# **Motion Commotion\!** {#motion-commotion!}

**Game Design Document (GDD)**

**Status:** Working Proposal — Subject to Team Approval  
**Platform:** Unity  
**Genre:** Educational Physics Puzzle / Simulation  
**Target Audience:** Middle School Students (Approximately Ages 11–14)  
**Development Timeline:** 4 Weeks  
**Art Direction:** Colorful, approachable 2D cartoon visuals inspired by *Red Ball*  
**Controls:** Mouse and Keyboard  
**Team:**   
Vincent Huang \- Graphics/Art,  
Trevor Magruder \- Metrics/Playtesting/Analytics,  
Jonathan Chu \- Developer, Product Manager  
Andy Leung \-  Graphics/Art,  
Sarah Nault \- Gameplay/Mechanics/Concepts,  
Timothy So \- Project Management  

---

[**Motion Commotion\!	1**](#motion-commotion!)

[1\. Game Overview	4](#1.-game-overview)

[Design Pillars	4](#design-pillars)

[2\. Narrative and Setting	4](#2.-narrative-and-setting)

[Setting	4](#setting)

[Player Objective	5](#player-objective)

[Narrative Integration	5](#narrative-integration)

[Narrative Scope	5](#narrative-scope)

[3\. Core Gameplay Loop	6](#3.-core-gameplay-loop)

[4\. Gameplay and Physics Systems	6](#4.-gameplay-and-physics-systems)

[4.1 Physics Puzzles	6](#heading=h.ywlm9bdegtj0)

[4.2 Simulation Controls	7](#4.2-simulation-controls)

[4.3 Formula Reference and Hints	7](#4.3-formula-reference-and-hints)

[5\. Difficulty System	8](#5.-difficulty-system)

[Easy	8](#easy)

[Medium	8](#medium)

[Hard	8](#hard)

[Attempt-Based Scoring	9](#attempt-based-scoring)

[Development Considerations	9](#development-considerations)

[6\. Scoring System	9](#heading=h.atxb7pz26zod)

[6.1 Main Puzzle Score	9](#heading=h.bax4dqujpy14)

[6.2 Absolute Outcome Error	10](#heading=h.q4ad3e83v81u)

[7\. Level Selection and Replayability	11](#heading=h.azmku6oachlz)

[Retry and Experimentation	11](#heading=h.iqglblkv301r)

[Fixed Lesson Variants	11](#heading=h.suzpkv2kdmu2)

[8\. Story and Player Progression	11](#8.-story-and-player-progression)

[Story Progression	11](#story-progression)

[Level Progression	12](#level-progression)

[High Scores and Replayability	12](#high-scores-and-replayability)

[Development Scope	12](#development-scope)

[9\. User Interface	12](#9.-user-interface)

[Main Menu	13](#main-menu)

[Challenge Screen	13](#challenge-screen)

[Results Screen	13](#results-screen)

[10\. Art Direction	13](#10.-art-direction)

[Visual Priorities	13](#visual-priorities)

[11\. Audio Design	14](#11.-audio-design)

[12\. Technical Design	14](#12.-technical-design)

[Implementation Priorities	14](#implementation-priorities)

[13\. Minimum Viable Product (MVP)	15](#13.-minimum-viable-product-\(mvp\))

[Required Features	15](#required-features)

[14\. Stretch Goals	15](#14.-stretch-goals)

[15\. Four-Week Development Plan	16](#15.-four-week-development-plan)

[Week 1 — Planning and Core Prototype	16](#week-1-—-planning-and-core-prototype)

[Week 2 — Block Programming and Scoring	16](#week-2-—-block-programming-and-scoring)

[Week 3 — Replayability and Playtesting	16](#week-3-—-replayability-and-playtesting)

[Week 4 — Refinement and Presentation	16](#week-4-—-refinement-and-presentation)

[16\. Playtesting and Evaluation	17](#16.-playtesting-and-evaluation)

[Key Evaluation Questions	17](#key-evaluation-questions)

[17\. Risks and Scope Management	17](#17.-risks-and-scope-management)

[Development Time	17](#development-time)

[Physics Accuracy	17](#physics-accuracy)

[Mathematical Accessibility	17](#mathematical-accessibility)

[Scoring Complexity	18](#scoring-complexity)

[Feature Creep	18](#feature-creep)

[18\. Open Team Decisions	18](#18.-open-team-decisions)

[19\. Project Vision	18](#19.-project-vision)

[**CURRICULUM AND BLOCK SYSTEM — CURRENT DESIGN BLUEPRINT	19**](#curriculum-and-block-system-—-current-design-blueprint)

[Status and scope	19](#status-and-scope)

[Core interaction and execution	19](#core-interaction-and-execution)

[Lesson 1 — Net Force and Acceleration	19](#lesson-1-—-net-force-and-acceleration)

[Lesson 2 — Average Velocity and Time	19](#lesson-2-—-average-velocity-and-time)

[Lesson 3 — Gravity and Inclined Planes	20](#lesson-3-—-gravity-and-inclined-planes)

[Lesson 4 — Projectile Motion	20](#lesson-4-—-projectile-motion)

[Lesson 5 — Einstein's Final Exam	20](#lesson-5-—-einstein's-final-exam)

[Conditional braking force field — proposed shared target mechanic	20](#conditional-braking-force-field-—-proposed-shared-target-mechanic)

[Scoring and mathematical validation	21](#scoring-and-mathematical-validation)

[Open implementation decisions	21](#open-implementation-decisions)

[**Theoretical Foundations and Design Applications	21**](#theoretical-foundations-and-design-applications)

[Overview	21](#overview)

[1\. Mechanics, Dynamics, and Aesthetics (MDA)	22](#1.-mechanics,-dynamics,-and-aesthetics-\(mda\))

[2\. GameFlow and Flow Theory	22](#2.-gameflow-and-flow-theory)

[3\. Self-Determination Theory	23](#3.-self-determination-theory)

[4\. Experiential Learning Theory	23](#4.-experiential-learning-theory)

[5\. Cognitive Load Theory	24](#5.-cognitive-load-theory)

[Theory-Based Playtesting and Evaluation	24](#theory-based-playtesting-and-evaluation)

[Conclusion	25](#conclusion)

[References	25](#references)

## **1\. Game Overview** {#1.-game-overview}

*Motion Commotion\! is an educational physics puzzle game designed to introduce middle school students to foundational physics concepts through block-based simulation, experimentation, and problem-solving.*

Players solve fixed physics challenges by arranging configurable Direction, Force, Duration, and Wait instruction blocks, running a simulation, and revising their sequence based on observed motion.

The game emphasizes learning through experimentation rather than memorization. Students are encouraged to develop confidence with physics equations, mathematical reasoning, and algebraic manipulation in a playful, low-pressure environment.

Set in the whimsical town of Motionville on the planet Inertia, *Motion Commotion\!* challenges players to restore order after the town's Commotion Engine malfunctions, causing widespread physics-related chaos. Players apply mathematical reasoning and physics principles to solve interactive 2D puzzles, gradually helping Motionville recover from the mayhem.

### **Design Pillars** {#design-pillars}

1. **Learn by Experimenting:** Students observe how changing physical variables affects outcomes.  
2. **Construct, Test, Improve: Students use physics formulas to plan block sequences, observe simulated outcomes, and refine their solutions.**  
3. **Accessible Learning:** Formulas, variable definitions, and hints support students without requiring prior physics expertise.  
4. **Reward Understanding: Scoring emphasizes accurate simulated outcomes, improvement, and personal high scores.**  
5. **Play Without Punishment:** Unlimited attempts allow students to experiment without being locked out of progression.

## **2\. Narrative and Setting** {#2.-narrative-and-setting}

### **Setting** {#setting}

*Motion Commotion\!* takes place in **Motionville**, a colorful, whimsical town located on the planet **Inertia**.

Motionville is home to the **Commotion Engine**, a powerful machine designed to keep the town's systems operating smoothly. Unfortunately, the engine has malfunctioned, causing widespread physics-related mayhem throughout the town.

Everyday activities have become chaotic. Objects move at incorrect speeds, deliveries miss their destinations, and machinery behaves unpredictably.

### **Player Objective** {#player-objective}

Players guide a sentient orb through physics challenges to recover five missing formulas and restore the Commotion Engine.

By applying real-world physics principles, players must overcome challenges caused by the malfunctioning Commotion Engine. Each puzzle presents a different situation that requires mathematical reasoning, programmable instruction blocks, and experimentation.

As players complete lessons, the orb recovers the missing formulas needed to restore the Commotion Engine.

### **Narrative Integration** {#narrative-integration}

The narrative provides context for the game's educational challenges without interrupting the core gameplay loop.

* Each lesson presents a physics-related obstacle on the orb's journey to recover a missing formula.  
* Players use calculations and simulations to determine an appropriate solution.  
* Successful completion represents progress toward restoring order.  
* More advanced challenges may represent increasingly complicated consequences of the engine's malfunction.

The game uses a lighthearted, humorous tone and colorful 2D environments. Narrative elements can be conveyed through brief introductory text, environmental details, and visual feedback rather than extensive dialogue or cutscenes.

### **Narrative Scope** {#narrative-scope}

**MVP:** Establish Motionville, the planet Inertia, and the malfunctioning Commotion Engine through a brief introduction. Connect the initial physics puzzle to a problem caused by the malfunction.

**Stretch Goals: Add humorous dialogue, environmental storytelling, and a more elaborate Commotion Engine restoration sequence. Einstein's final exam is part of the planned five-lesson curriculum, not an extra combat encounter.**

The narrative supports the educational gameplay while remaining achievable within the four-week development schedule.

## **3\. Core Gameplay Loop** {#3.-core-gameplay-loop}

Each physics puzzle follows a consistent gameplay sequence.

**Step 1: Present the Challenge**

The player sees the fixed lesson environment, a clear positional or timed objective, and the available instruction blocks. A brief tutorial introduces new block functions and physics concepts as needed.

Example: Arrange Direction, Force, Duration, and Wait blocks so the orb reaches a timed gateway at the required time.

**Step 2: Adjust Variables and Design Block-Coded Action Sequence**

The player arranges and configures available Direction, Force, Duration, and Wait blocks to control the orb's motion. The available blocks and their numerical settings depend on the lesson.

**Step 3: Run the Simulation**

The player runs the configured block sequence and observes the orb's motion. Relevant forces and parameters are shown where practical; playback controls are optional polish.

**Step 4: Compare Results**

The game displays the simulated result, positional or arrival-time accuracy as appropriate to the lesson, and points earned.

**Step 5: Retry or Continue**

Players may retry the same fixed variant, select another difficulty, or continue to the next unlocked lesson. Completing a lesson on any difficulty unlocks the next lesson.

**Optional stretch: Formula Flip offers separate equation-rearrangement practice without awarding points or affecting lesson unlocks.**

## **4\. Gameplay and Physics Systems** {#4.-gameplay-and-physics-systems}

### 4.1 Physics Puzzles

Motion Commotion\! uses five sequential lessons, each with a fixed Easy, Medium, and Hard variant. Players construct sequences of Direction, Force, Duration, and Wait blocks, then run a Unity 2D physics simulation to reach a positional target or timed gateway. Lessons introduce new concepts while retaining previously learned physics where applicable. Each puzzle must be mathematically solvable from the displayed formulas and known parameters.

**Lesson 1 — Net Force and Acceleration (F\_net \= ma).** Easy: level track with low friction. Medium: increased friction. Hard: increased friction plus opposing wind. Goal: reach a positional target.

**Lesson 2 — Average Velocity and Time (v\_avg \= Δx/Δt).** Easy: short timed gateway with generous arrival tolerance. Medium: farther gateway. Hard: same farther gateway with tighter arrival-time tolerance. Goal: arrive near the specified time.

**Lesson 3 — Gravity and Inclined Planes (F\_parallel \= mg sin θ in a simplified sliding model).** Easy: gentle downhill. Medium: steeper downhill. Hard: downhill followed by uphill. Goal: reach a positional target while accounting for gravity and friction.

**Lesson 4 — Projectile Motion (horizontal and vertical displacement).** Easy: short ravine with level landing. Medium: wider ravine. Hard: target at a different elevation. Goal: land within the target zone.

**Lesson 5 — Einstein's Final Exam. A single continuous simulation combines prior concepts.** Easy: downhill and short jump. Medium: slope, timed gateway, and jump. Hard: adds opposing wind and tighter landing requirements. Einstein is a narrative boss/puzzle examiner, not a combat enemy.

For position-based objectives, a proposed conditional braking field captures the orb only when its entry speed and remaining stopping distance allow a physically valid stop. Timed gateway scoring instead measures absolute arrival-time error. See “Curriculum and Block System — Current Design Blueprint” for block semantics, capture calculations, detailed variants, and open physics assumptions.

Four-week implementation scope: prioritize one complete, polished, mathematically verified playable lesson as the MVP. The five-lesson sequence and all fifteen variants define the full design; implementation of additional content depends on time and playtesting.

### **4.2 Simulation Controls** {#4.2-simulation-controls}

The game will use a 2D physics environment featuring simple, expressive objects and animations.

Players will interact primarily by configuring experiments rather than directly controlling a character.

**Planned Inputs:**

* Mouse controls for buttons, sliders, and numerical fields  
* Keyboard input for block values and interface navigation  
* A simulation start button  
* Retry and Continue options

**Proposed:** Additional keyboard shortcuts for common actions.

### **4.3 Formula Reference and Hints** {#4.3-formula-reference-and-hints}

Each challenge displays the relevant physics formula and defines all variables; no numerical prediction-entry mechanic is required.

Example:

**Average Velocity \= Distance ÷ Time**

V\_avg \= d / t

Where:

* V\_avg \= average velocity (m/s)  
* d \= displacement (m), for a straight-line scenario  
* t \= elapsed time (s)

The relevant formula remains visible during block construction and simulation. Formula rearrangement can be practiced separately through optional Formula Flip; it is not a difficulty gate.

A Hint button helps students identify relevant variables, select useful blocks, and reason through the physical relationships without revealing the full solution.

The intention is to teach students how to work with formulas rather than simply enter memorized answers.

## **5\. Difficulty System** {#5.-difficulty-system}

*Each lesson has three fixed difficulty variants (Easy, Medium, Hard), distinguished by physical conditions, course geometry, and/or outcome tolerance. The variants build on earlier lessons rather than requiring formula rearrangement or unit conversion as difficulty gates. See “Curriculum and Block System — Current Design Blueprint” for the lesson-by-lesson specification.*

All three variants use the same instruction-block mechanics and relevant physics formulas. Difficulty is designed into each fixed scenario, not generated by randomizing numerical inputs. Students build, test, and revise a block sequence; no numerical prediction field is used.

### **Easy** {#easy}

* Easy introduces the lesson's new physics principle in the simplest terrain or timing conditions, with forgiving tolerances.  
* Lesson 1: low-friction level path. Lesson 2: short timed gateway. Lesson 3: gentle downhill slope. Lesson 4: short same-elevation ravine. Lesson 5: downhill slope followed by a short jump.  
* Students use the displayed formula and known parameters to plan a block sequence and observe its result.  
* Values and units remain clear and consistent across difficulties.

### **Medium** {#medium}

* Medium adds a physical challenge or increases distance relative to Easy.  
* Lesson 1: higher friction. Lesson 2: farther timed gateway. Lesson 3: steeper downhill. Lesson 4: wider ravine. Lesson 5: slope, timed gateway, and jump in one simulation.  
* Values and units remain clear and consistent across difficulties.

### **Hard** {#hard}

* Hard adds another physical condition, more demanding terrain, or tighter accuracy tolerance.  
* Hard builds on the lesson's prior concepts with more demanding physical conditions or tighter tolerances, as specified below.  
* Lesson 1: Medium's friction plus opposing wind. Lesson 2: Medium's gateway distance with tighter timing tolerance. Lesson 3: downhill then uphill. Lesson 4: target at different elevation. Lesson 5: timed gateway, opposing wind, and more demanding landing in one simulation.

### **Attempt-Based Scoring** {#attempt-based-scoring}

All difficulty settings use the same attempt multipliers:

* First attempt: 2× points  
* Second attempt: 1.5× points  
* Third and subsequent attempts: 1× points

Players have unlimited attempts regardless of difficulty. Retry replays the same fixed level variant and advances its attempt counter; players may select a different fixed difficulty variant, but there is no randomized New Challenge action.

Outcome accuracy is measured from the simulated resting position or gateway arrival time. Exact thresholds and multipliers remain subject to team approval.

### **Development Considerations** {#development-considerations}

Difficulty variants should reuse Unity scenes, physics components, block execution, and assets where practical. Configure friction, wind, slopes, distances, target locations, and timing tolerances per fixed variant. See the curriculum blueprint for exact conceptual differences; numerical parameters remain to be verified.

The MVP will prioritize one fully functional physics puzzle and its core learning loop. Additional difficulty configurations may be implemented incrementally as development time allows.

## **6\. Scoring System**

Scoring rewards successful completion of the main physics challenge and accurate simulated outcomes. Accuracy uses absolute differences in physical units rather than percentages. Formula Flip is optional practice and does not award separate points.

### **6.1 Main Puzzle Scor**e

Main Puzzle Score \= Completion Points × Outcome Accuracy Multiplier × Attempt Multiplier

Completion points are awarded for a successful run. The outcome accuracy multiplier applies to those points; a failed run does not earn completion points. The attempt multiplier is shared across all difficulties: first attempt 2×, second attempt 1.5×, third and subsequent attempts 1×. Players may retry indefinitely. The best score for each lesson and difficulty variant is retained.

### **6.2 Absolute Outcome Erro**r

Position-based challenges: Position Error \= \|x\_final − x\_target\|, measured in meters along the relevant scoring axis. For a landing target, use the landing position; for a braking-field target, use the orb's final resting position.

Timed gateways: Time Error \= \|t\_arrival − t\_target\|, measured in seconds.

Absolute value makes overshooting and undershooting by the same amount equally accurate. A target at 10 m yields errors of 1 m at 9 m or 11 m, 2 m at 8 m or 12 m, and 3 m at 7 m or 13 m.

### **6.3 Proposed Accuracy Band**s

Illustrative positional bands only: error ≤ 1 m earns 2.0×; error \> 1 m and ≤ 2 m earns 1.5×; error \> 2 m and ≤ 3 m earns 1.25×; error \> 3 m earns 1.0× if the run still qualifies as a success. Timed levels use analogous thresholds in seconds, chosen for that specific level. These thresholds and multipliers remain proposals pending physics validation and playtesting.

Each fixed lesson variant defines its own success conditions and accuracy thresholds. A run can satisfy the level's completion criteria without earning the highest accuracy tier. Conversely, a close result that violates a required condition (such as missing a gateway or failing the physically valid target capture) is not automatically a success. The team must document which position or event is measured, the capture/landing tolerance, and the threshold boundaries.

### **6.4 Implementation and Feedbac**k

Display the target value, actual result, absolute error with units, accuracy tier, attempt multiplier, points earned, and personal best. Apply rounding only for display; evaluate thresholds against the unrounded simulation result. Einstein's continuous final exam may need an explicit rule for combining multiple objectives; do not assume one universal distance metric covers both timing and landing.

## **7\. Level Selection and Replayability**

### **Fixed Lesson Variant**s

Motion Commotion\! has five sequential lessons, each with three designed difficulty variants: Easy, Medium, and Hard. Each variant changes relevant terrain, environmental forces, distance, or success tolerance. Section 5 summarizes difficulty progression; Section 4.1 and “Curriculum and Block System — Current Design Blueprint” specify the lesson-by-lesson conditions.

### **Lesson Progressio**n

Players progress through Lessons 1–5 by successfully completing physics challenges. Each lesson introduces a new concept and may build on mechanics learned earlier. The final lesson is Einstein's continuous multi-concept challenge. Completing a lesson on any difficulty (Easy, Medium, or Hard) unlocks the next lesson. Higher difficulties are optional challenges for improved high scores, not progression gates; no separate leveling system is used.

### **Retry and Experimentatio**n

Players may retry a fixed lesson variant without an attempt limit. Retry preserves the variant's geometry, physics parameters, and objective so players can revise their Direction, Force, Duration, and Wait blocks and learn from the previous simulation. Each new run advances the attempt counter for that challenge. There is no randomized New Challenge button or automatic parameter reroll.

### **High Score**s

Each lesson and difficulty variant retains the player's highest score. Replays can improve that record, but repeated lower-scoring attempts do not accumulate points toward a separate Physics Level. The established attempt multipliers are 2× on the first attempt, 1.5× on the second, and 1× thereafter. Outcome accuracy is based on stopping position for positional targets or arrival-time error for timed gateways; exact score thresholds are still to be confirmed in Section 6\.

### **Replayability and Scop**e

Replayability comes from trying harder fixed variants, improving block sequences, achieving more accurate results, and beating personal high scores. A leaderboard is optional, not required for progression. The four-week MVP prioritizes a polished, validated lesson and a reliable Retry loop; the full five-lesson, fifteen-variant structure is the intended curriculum rather than a guaranteed four-week deliverable.

## **8\. Story and Player Progression** {#8.-story-and-player-progression}

### **Story Progression** {#story-progression}

*Motion Commotion\!* follows a sentient orb created by the Commotion Engine, which has malfunctioned after losing five essential physics formulas. In this world, mathematical formulas exist as physical objects, and their absence has caused widespread chaos throughout Motionville.

The orb must navigate a series of physics-based challenges to recover the missing formulas and return them to the Commotion Engine.

Each recovered formula represents a narrative milestone and progress toward restoring order to Motionville.

### **Level Progression** {#level-progression}

Players advance sequentially through Lessons 1–5 by applying mathematical reasoning, arranging instruction blocks, and successfully completing simulations. Completing any lesson on Easy, Medium, or Hard unlocks the next lesson; higher difficulties are optional challenges, not progression gates.

Each challenge reinforces a physics concept while contributing to the orb's larger mission.

The narrative provides a sense of purpose and accomplishment without requiring extensive cutscenes, dialogue, or exploration mechanics.

### **High Scores and Replayability** {#high-scores-and-replayability}

The game progresses through Lessons 1–5. A high-score system records the best result for each completed lesson and difficulty variant, encouraging replay without adding a separate experience-level system.

* Each lesson and difficulty variant retains its own high score; a combined leaderboard or total score is optional and not required for progression.  
* Replaying a challenge can replace its previous high score when the player performs better.  
* Scores come exclusively from the main physics challenge, with replayability driven by improved personal high scores. Optional Formula Flip practice is separate from scoring and lesson unlocks.  
* Repeating a challenge cannot inflate its stored high score; only the best attempt is retained.

High scores are performance records only. Story progression follows successful completion of the five lessons.

### **Development Scope** {#development-scope}

**MVP:** Introduce the orb's mission, establish the missing-formula premise, and connect successful puzzle completion to narrative progress.

**Stretch Goals: Expand narrative milestones and the Commotion Engine restoration sequence; offer Formula Flip as an optional equation-rearrangement activity; add a polished high-score display or leaderboard if time allows. Formula Flip does not gate lesson progression or award separate bonus points.**

## **9\. User Interface** {#9.-user-interface}

The interface should be visually friendly, readable, and approachable for middle school students.

### **Main Menu** {#main-menu}

* Play  
* Difficulty Selection  
* Instructions / How to Play

**High-score display for completed lessons (stretch: leaderboard).**

### **Challenge Screen** {#challenge-screen}

* Current physics objective  
* Interactive 2D simulation area  
* Adjustable physical variables  
* Formula reference and variable definitions  
* Block sequence workspace with configurable values  
* Hint button  
* Start Simulation button

### **Results Screen** {#results-screen}

* Configured block sequence  
* Actual simulation result  
* Outcome accuracy  
* Points earned  
* Retry button  
* Difficulty selection / level selection button  
* Continue button, when another level is available

**Stretch: Optional Formula Flip activity, polished results presentation, high-score feedback, and lesson-unlock animations.**

## **10\. Art Direction** {#10.-art-direction}

*Motion Commotion\!* will use a colorful 2D cartoon art style inspired by the approachable visual language of *Red Ball*.

The presentation should be playful, expressive, and easy to understand.

### **Visual Priorities** {#visual-priorities}

* Bright, contrasting colors  
* Simple geometric characters and objects  
* Clear platforms, ramps, obstacles, and targets  
* Readable numerical labels and controls  
* Simple animations that communicate motion and physical reactions  
* Visual feedback for successful target capture, timed-gateway arrival, and completed objectives

The art should support the educational experience without overwhelming students with unnecessary visual detail.

The project will use original or appropriately licensed assets rather than directly copying existing game artwork.

**Proposed:** A friendly ball character or mascot may appear in simulations to give the game additional personality.

## **11\. Audio Design** {#11.-audio-design}

**Stretch Goal**

Potential audio features include:

* A short sound when a simulation begins  
* Collision and bouncing effects  
* Positive feedback for completing objectives  
* Distinct sounds for setting a new personal high score  
* Optional background music

Audio should be supportive rather than distracting.

## **12\. Technical Design** {#12.-technical-design}

**Target Platform:** Modern desktop

**Application Type:** Unity game

**Rendering:** 2D

**Input:** Mouse and keyboard

**Technology Stack:** To Be Determined by the Team

Implementation uses Unity 2D; programming language, physics tuning, and supporting architecture are determined by the development team.

The team should select technology based on familiarity, physics requirements, and the four-week development schedule.

### **Implementation Priorities** {#implementation-priorities}

* Reusable physics puzzle interface  
* Data-driven configurations for fixed lesson and difficulty variants  
* Consistent block construction, simulation, outcome feedback, and Retry workflow  
* Reliable mathematical calculations  
* Clear separation between simulation logic and scoring logic

**Proposed: Store fixed lesson and difficulty configurations as reusable Unity data objects, so terrain, forces, timing tolerances, and available blocks can vary without rebuilding the interface.**

## **13\. Minimum Viable Product (MVP)** {#13.-minimum-viable-product-(mvp)}

The MVP is one complete, playable educational physics puzzle demonstrating the game's central learning experience.

### **Required Features** {#required-features}

1. One physics concept selected by the team.  
2. One functioning 2D physics simulation.  
3. Adjustable physical variables.  
4. Configurable Direction, Force, Duration, and Wait instruction blocks.  
5. Visible formula reference, variable definitions, and optional hint.  
6. Simulation results measured against the target position or target arrival time.  
7. Basic outcome-accuracy scoring and shared attempt multipliers across all difficulties.  
8. Unlimited retries.  
9. A mathematically validated fixed challenge; Easy, Medium, and Hard variants are part of the full curriculum blueprint.  
10. Simple, readable interface with mouse and keyboard support.

**Success Criterion: A student can understand the objective, construct and configure a block sequence, run the simulation, interpret the result, and retry without developer assistance.**

## **14\. Stretch Goals** {#14.-stretch-goals}

If the MVP is completed and tested, additional features may be implemented in the following suggested priority order.

| Priority | Feature |
| ----- | ----- |
| High | Additional physics puzzles, targeting three total |
| High | Optional Formula Flip activity (stretch goal; separate from main challenge scoring) |
| Medium | Additional interface accessibility and tutorial improvements |
| Medium | Additional validated fixed difficulty variants |
| Medium | High-score records and optional leaderboard |
| Low | Expanded animations and audio |
| Low | Additional visual customization and polish |

The team may revise these priorities based on development progress and playtesting.

## **15\. Four-Week Development Plan** {#15.-four-week-development-plan}

### **Week 1 — Planning and Core Prototype** {#week-1-—-planning-and-core-prototype}

* Finalize the initial physics concept.  
* Configure the Unity 2D project and establish the physics simulation architecture.  
* Define the puzzle's equations and numerical variables.  
* Establish the basic interface and visual direction.  
* Create an initial working simulation.

**Milestone:** A basic physics scenario can be configured and simulated.

### **Week 2 — Block Programming and Scoring** {#week-2-—-block-programming-and-scoring}

* Implement drag-and-drop Direction, Force, Duration, and Wait blocks with configurable values.  
* Display formulas, variable definitions, and hints.  
* Execute block sequences and compare simulated stopping positions or gateway arrival times with the level objective.  
* Implement outcome-based scoring for target position or gateway arrival time.  
* Implement shared attempt multipliers and high-score tracking.

**Milestone: The complete block sequence → simulation → outcome feedback → Retry loop works.**

### **Week 3 — Replayability and Playtesting** {#week-3-—-replayability-and-playtesting}

* Validate the chosen MVP challenge mathematically; build additional fixed difficulty variants only if time permits.  
* Add Retry and fixed difficulty-variant selection functionality.  
* Test difficulty settings and scoring behavior.  
* Conduct initial usability testing.  
* Resolve mathematical inconsistencies and gameplay issues.

**Milestone:** A playable MVP is ready for external feedback.

### **Week 4 — Refinement and Presentation** {#week-4-—-refinement-and-presentation}

* Improve interface clarity and visual feedback.  
* Fix bugs and balance scoring.  
* Validate physics calculations and numerical tolerances.  
* Add stretch features only if time permits.  
* Prepare the final demonstration and presentation.

**Milestone:** A polished, stable educational physics puzzle is ready for demonstration.

## **16\. Playtesting and Evaluation** {#16.-playtesting-and-evaluation}

The game should be tested with users representative of its target audience when possible.

### **Key Evaluation Questions** {#key-evaluation-questions}

* Can students understand what the puzzle asks them to do?  
* Are formulas and variable definitions easy to interpret?  
* Can students arrange, configure, and run the instruction blocks without assistance?  
* Can students explain why their simulated outcome met or missed the target?  
* Do hints support learning without giving away solutions?  
* Does the scoring system encourage experimentation?  
* Are the controls and interface intuitive?  
* Do students feel encouraged to revise their block sequence after an unsuccessful attempt?

Testing should combine observation with brief player feedback.

The team should prioritize correcting confusing mechanics and mathematical errors over adding new content.

## **17\. Risks and Scope Management** {#17.-risks-and-scope-management}

### **Development Time** {#development-time}

Four weeks is a limited development period. Additional physics concepts should not be implemented at the expense of completing the core learning loop.

### **Physics Accuracy** {#physics-accuracy}

The game's simulation must agree with its educational equations and stated assumptions. Simplified physics is acceptable when clearly explained and mathematically consistent.

### **Mathematical Accessibility** {#mathematical-accessibility}

Numerical values, rounding requirements, and formulas should be appropriate for middle school students.

### **Scoring Complexity** {#scoring-complexity}

Outcome accuracy bands and attempt multipliers may be difficult to balance within the available development time. Simplify scoring if needed while preserving clear feedback and unlimited retries.

### **Feature Creep** {#feature-creep}

Additional lesson variants, leaderboard features, and cosmetics should remain optional until the MVP is stable.

## **18\. Open Team Decisions** {#18.-open-team-decisions}

The following decisions require team discussion and approval:

1. Which physics concept will be implemented first?  
2. Which Unity 2D physics components, project settings, and block-execution architecture will be used?  
3. What specific physical variables can students manipulate in the first puzzle?  
4. Which block types and parameter ranges will the MVP lesson provide, and how will the game explain them?  
5. How will positional and arrival-time accuracy, numerical tolerances, and score thresholds be calculated?  
6. Which proposed scoring mechanics are essential for the MVP?  
7. How will tasks be distributed across the development team?  
8. What stretch features are realistic after the MVP is complete?

## **19\. Project Vision** {#19.-project-vision}

*Motion Commotion\!* aims to make physics approachable by turning abstract equations into interactive, understandable experiences.

Rather than simply telling students how physics works, the game invites them to construct block-based solutions, experiment with forces and timing, observe simulated results, and develop confidence through practice.

**Our goal is not to make students memorize physics equations. It is to help them discover that they can understand and use them.**

The project will prioritize one polished, educationally meaningful gameplay experience over a larger collection of unfinished features.

# CURRICULUM AND BLOCK SYSTEM — CURRENT DESIGN BLUEPRINT {#curriculum-and-block-system-—-current-design-blueprint}

## Status and scope {#status-and-scope}

The full design contains five sequential physics lessons, each with fixed Easy, Medium, and Hard variants (15 variants total). Lessons take the orb progressively deeper into the Earth. The fifth lesson culminates in a continuous boss puzzle featuring Albert Einstein, as selected by the team. This is the full curriculum plan, not a commitment to implement all 15 variants during the four-week project. The MVP remains one polished, playable, mathematically verified physics challenge; additional lessons and variants depend on team capacity and testing.

## Core interaction and execution {#core-interaction-and-execution}

Players assemble and configure a sequence of instruction blocks, run the entire sequence as a physics simulation, observe the result, and revise their solution. The level offers only block types relevant to its challenge, in no prescribed solution order. Players may use multiple copies of any available block. Direction sets or replaces the current force angle in degrees. Force sets or replaces the current applied-force magnitude in newtons. Duration applies the configured force for a specified number of simulation seconds. Wait advances simulation time without additional player-controlled force. Later Direction or Force blocks replace the earlier settings rather than adding to them; changing settings does not reset the orb's existing velocity. Environmental friction, gravity, and wind act independently throughout. Exact block execution edge cases and the per-level palette require prototype verification.

## Lesson 1 — Net Force and Acceleration {#lesson-1-—-net-force-and-acceleration}

Primary equation: F\_net \= ma. Objective: reach a target along a straight, level path. Easy: low friction, no wind. Medium: higher friction, no wind. Hard: the same higher friction as Medium plus opposing wind. Learning progression: apply force; account for friction; combine multiple opposing forces. Target accuracy is positional.

## Lesson 2 — Average Velocity and Time {#lesson-2-—-average-velocity-and-time}

Primary equation: v\_avg \= Δx / Δt for straight-line displacement. Objective: reach an underground gateway close to a specified simulation arrival time. Easy: short straight passage with generous timing tolerance. Medium: more distant gateway. Hard: Medium's distance with tighter arrival-time tolerance. Previously learned friction and force behavior remain relevant. Scoring uses absolute arrival-time error (early and late arrivals are treated symmetrically). Exact target times and tolerance bands remain TBD.

## Lesson 3 — Gravity and Inclined Planes {#lesson-3-—-gravity-and-inclined-planes}

Primary concept: gravity's component along a slope; a simplified sliding model may use F\_parallel \= mg sin(θ), with friction accounted for separately. Easy: gentle downhill slope. Medium: steeper downhill slope. Hard: downhill followed by uphill terrain. Objective: reach the target while accounting for how terrain changes acceleration. Positional accuracy applies. Sliding versus rotational rolling physics must be explicitly aligned with the equations presented to students.

## Lesson 4 — Projectile Motion {#lesson-4-—-projectile-motion}

Primary concepts: horizontal and vertical displacement during flight; simplified equations Δx \= v\_x t and Δy \= v\_y0 t − (1/2)gt², when assumptions apply. Easy: short ravine, landing at the launch elevation. Medium: wider ravine. Hard: target at a different elevation. Objective: configure a launch that lands within the target region. This is the major two-dimensional step in the curriculum; hints and known values should keep the mathematics accessible.

## Lesson 5 — Einstein's Final Exam {#lesson-5-—-einstein's-final-exam}

Albert Einstein is the team's selected final boss. Each difficulty is ONE continuous simulation, not a series of disconnected minigames. The lesson assesses cumulative use of net force, timing, gravity on slopes, and projectile motion. Proposed Easy: incline followed by a short ravine jump. Proposed Medium: slope, timed gateway, and ravine crossing. Proposed Hard: Medium's course plus opposing wind and more precise landing requirements. Einstein's presentation and reactions can convey the boss encounter without a combat system. Exact boss scoring and level geometry remain TBD.

## Conditional braking force field — proposed shared target mechanic {#conditional-braking-force-field-—-proposed-shared-target-mechanic}

For position-based targets, the target region may contain a non-bouncing braking force field. It should capture the orb only if the entry location and speed permit it to stop within the available field distance; otherwise the attempt continues or fails under ordinary physics. In a one-dimensional constant-net-deceleration model, d\_stop \= v\_entry² / (2a\_brake), where a\_brake is a positive deceleration magnitude. Capture is possible when v\_entry² ≤ 2a\_brake d\_available. The field must stop braking at zero velocity so the orb does not reverse. Captured final resting position determines positional accuracy. The field is a simplifying design proposal, not a finalized Unity implementation; angled flight, other environmental forces, and target geometry require explicit treatment. Level 2 gateway arrival timing is measured independently.

## Scoring and mathematical validation {#scoring-and-mathematical-validation}

Successful attempts retain the shared attempt multipliers: first 2×, second 1.5×, third and later 1×, with unlimited retries. Outcome accuracy replaces numerical prediction accuracy: final position relative to the target for position-based levels, and \|t\_arrival − t\_target\| for the timed gateway. Exact accuracy bands and boss combination rules are not yet approved. Each variant must document its learning objective, relevant formula, given values, unknowns, physics assumptions, fixed geometry, available blocks, intended solution sequence, verified calculation, and scoring/capture tolerances. Prefer simple, transparent formulas over trial-and-error-only puzzles. Numerical prediction-entry fields, formula-rearrangement difficulty tiers, and randomized A/B/C numerical scenarios are not part of this design.

## Open implementation decisions {#open-implementation-decisions}

Confirm force application and collision behavior in Unity FixedUpdate; whether the orb slides or rolls with rotational inertia; friction coefficients and wind strength; per-level block palette; force-field braking model and capture geometry; exact timing and positional accuracy tiers; feasibility of all five lessons within the schedule; and Einstein's final scoring. Treat these as design/engineering tasks rather than approved numerical specifications.

# **Theoretical Foundations and Design Applications** {#theoretical-foundations-and-design-applications}

## **Overview** {#overview}

*Motion Commotion\!* incorporates established game design frameworks and research-based learning theories to support engagement, experimentation, and mathematical understanding.

The game's core systems emphasize player choice, adjustable difficulty, immediate feedback, and learning through repeated experimentation. These design decisions are informed by the following theoretical foundations.

### **1\. Mechanics, Dynamics, and Aesthetics (MDA)** {#1.-mechanics,-dynamics,-and-aesthetics-(mda)}

**Research:** Hunicke, LeBlanc, and Zubek (2004)

The MDA framework examines games through three interconnected components: mechanics, dynamics, and aesthetics. Mechanics describe the game's rules and systems, dynamics describe how players interact with those systems, and aesthetics describe the experiences those interactions are intended to produce.

**Application to Motion Commotion\!:**

* **Mechanics:** Players manipulate physics variables, predict numerical outcomes, run simulations, and earn scores based on performance.  
* **Dynamics: Players experiment with block sequences and values, compare simulated outcomes against level objectives, and refine their solutions through repeated attempts.**  
* **Aesthetics:** The intended experiences include curiosity, discovery, challenge, accomplishment, and satisfaction from overcoming obstacles.

The narrative reinforces these experiences by giving players a meaningful objective: guiding a sentient orb through Motionville to recover five missing formulas and restore the Commotion Engine.

The MDA framework helps the development team evaluate whether the implemented mechanics produce the intended player experience.

### **2\. GameFlow and Flow Theory** {#2.-gameflow-and-flow-theory}

**Research:** Sweetser and Wyeth (2005), building on Csikszentmihalyi's Flow Theory.

GameFlow identifies elements associated with player enjoyment, including clear goals, appropriate challenges, player skills, control, and feedback.

**Application to Motion Commotion\!:**

The game's difficulty system is designed to accommodate different levels of mathematical proficiency:

* **Easy: Simpler physical conditions and forgiving target or timing tolerances, with consistent units and visible formulas.**  
* **Medium: increased distance, friction, slope, or combined mechanics depending on the lesson.**  
* **Hard: additional environmental forces, more complex terrain, or tighter tolerances depending on the lesson.**

Players can select an appropriate challenge without changing the fundamental physics concept being taught.

Clear objectives establish what players need to accomplish, while immediate simulation results provide feedback about their decisions.

Unlimited retries allow players to improve their understanding without being permanently penalized for mistakes.

These systems are intended to balance challenge and player ability, reducing unnecessary frustration while preserving meaningful difficulty.

### **3\. Self-Determination Theory** {#3.-self-determination-theory}

**Research:** Ryan, Rigby, and Przybylski (2006)

Self-Determination Theory identifies autonomy, competence, and relatedness as fundamental psychological needs associated with motivation. Research applying this theory to video games has connected perceived autonomy and competence with player enjoyment.

**Application to Motion Commotion\!:**

**Autonomy: Players choose among available difficulty variants, experiment with block sequences and variables, and decide when to retry or replay completed lessons.**

**Competence: Players receive feedback on simulated outcomes and can improve their high scores as their mathematical understanding develops.**

**Relatedness:** The narrative may encourage a sense of connection to the sentient orb and the fictional world of Motionville, although this aspect is less central to the design and would require playtesting to evaluate.

High-score records support competence by recognizing improved performance without introducing a separate leveling system.

These systems aim to make mathematical problem-solving feel self-directed and rewarding.

### **4\. Experiential Learning Theory** {#4.-experiential-learning-theory}

**Research:** Kolb (1984)

Experiential Learning Theory describes learning as a process involving concrete experience, reflective observation, abstract conceptualization, and active experimentation.

**Application to Motion Commotion\!:**

The game's block-building and simulation loop encourages players to engage in a similar learning process:

1. **Planning: Players use physics concepts and formulas to configure a block sequence.**  
2. **Experimentation:** Players configure variables and run the simulation.  
3. **Observation: Players inspect motion and compare the outcome with the level's target.**  
4. **Reflection and Revision:** Players identify discrepancies, reconsider their calculations, and adjust their approach.

Unlimited retries allow this process to repeat as players refine their understanding.

The game is intended to encourage active engagement with physics principles rather than relying exclusively on memorization or passive instruction.

### **5\. Cognitive Load Theory** {#5.-cognitive-load-theory}

**Research:** Sweller (1988)

Cognitive Load Theory examines how the limitations of working memory influence learning and problem-solving. Instructional design can support learning by reducing unnecessary mental demands while maintaining meaningful cognitive challenges.

**Application to Motion Commotion\!:**

The game's interface and difficulty settings are designed to manage unnecessary complexity.

* Relevant formulas remain accessible during challenges, reducing the need to memorize equations before applying them.  
* Variables are clearly labeled to help players understand their mathematical relationships.  
* Hints provide guidance without automatically supplying solutions.  
* Easy difficulty introduces each lesson's new physics principle through simpler terrain and forgiving conditions.  
* Medium and Hard progressively add physical complexity or tighter outcome tolerances without requiring unrelated algebra or unit conversions.

These features are intended to help players concentrate on the physics concepts being taught rather than struggling with unclear instructions or excessive information.

The game should avoid presenting unnecessary visual distractions or excessive information during mathematical problem-solving.

## **Theory-Based Playtesting and Evaluation** {#theory-based-playtesting-and-evaluation}

The development team can use these theoretical foundations to evaluate whether the gameplay achieves its intended objectives.

| Framework | Evaluation Focus |
| ----- | ----- |
| MDA | Whether mechanics produce experimentation, curiosity, and satisfaction |
| GameFlow | Whether challenge levels, goals, and feedback support engagement |
| Self-Determination Theory | Whether players experience choice, competence, and motivation |
| Experiential Learning | Whether players use feedback to revise block sequences and improve understanding |
| Cognitive Load Theory | Whether instructions, formulas, and interface elements are understandable without unnecessary confusion |

Playtesting may incorporate observation, questionnaires, outcome accuracy, retry patterns, and short conceptual questions.

These measures can help identify design weaknesses and inform iterative improvements.

Improved scores alone should not be interpreted as definitive evidence of learning. Where feasible, conceptual understanding should be evaluated separately from gameplay performance.

## **Conclusion** {#conclusion}

The design of *Motion Commotion\!* draws upon established research to connect educational objectives with engaging gameplay.

By combining adjustable difficulty, player autonomy, immediate feedback, and repeated experimentation, the game aims to create an accessible environment for practicing physics and mathematical reasoning.

Theoretical frameworks provide guidance for design decisions and a foundation for evaluating the finished prototype. Their application does not guarantee player engagement or educational effectiveness; those outcomes must be investigated through playtesting and iteration.

## **References** {#references}

Hunicke, R., LeBlanc, M., & Zubek, R. (2004). *MDA: A formal approach to game design and game research*. Proceedings of the AAAI Workshop on Challenges in Game AI.

Kolb, D. A. (1984). *Experiential learning: Experience as the source of learning and development*. Prentice Hall.

Ryan, R. M., Rigby, C. S., & Przybylski, A. (2006). The motivational pull of video games: A self-determination theory approach. *Motivation and Emotion, 30*, 344–360. [https\://doi.org/10.1007/s11031-006-9051-8](https://doi.org/10.1007/s11031-006-9051-8)

Sweetser, P., & Wyeth, P. (2005). GameFlow: A model for evaluating player enjoyment in games. *Computers in Entertainment, 3*(3), Article 3A. [https\://doi.org/10.1145/1077246.1077253](https://doi.org/10.1145/1077246.1077253)

Sweller, J. (1988). Cognitive load during problem solving: Effects on learning. *Cognitive Science, 12*(2), 257–285. [https\://doi.org/10.1207/s15516709cog1202\_4](https://doi.org/10.1207/s15516709cog1202_4)

