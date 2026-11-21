# **Motion Commotion\!** {#motion-commotion!}

**Game Design Document (GDD)**

**Status:** Working Proposal — Subject to Team Approval  
**Platform:** Web Application (Browser-Based)  
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

[4.1 Physics Puzzles	6](#4.1-physics-puzzles)

[4.2 Simulation Controls	7](#4.2-simulation-controls)

[4.3 Formula Reference and Hints	7](#4.3-formula-reference-and-hints)

[5\. Difficulty System	8](#5.-difficulty-system)

[Easy	8](#easy)

[Medium	8](#medium)

[Hard	8](#hard)

[Attempt-Based Scoring	8](#attempt-based-scoring)

[Development Considerations	9](#development-considerations)

[6\. Scoring System	9](#6.-scoring-system)

[6.1 Main Puzzle Score	9](#6.1-main-puzzle-score)

[6.2 Prediction Accuracy	9](#6.2-prediction-accuracy)

[6.3 Optional Physics Objectives	10](#6.3-optional-physics-objectives)

[6.4 Post-Level Bonus Challenges	10](#6.4-post-level-bonus-challenges)

[7\. Challenge Variations and Replayability	10](#7.-challenge-variations-and-replayability)

[Retry	11](#retry)

[New Challenge	11](#new-challenge)

[8\. Story and Player Progression	11](#8.-story-and-player-progression)

[Story Progression	11](#story-progression)

[Level Progression	11](#level-progression)

[Player Progression (Stretch Goal)	12](#player-progression-\(stretch-goal\))

[Development Scope	12](#development-scope)

[9\. User Interface	12](#9.-user-interface)

[Main Menu	12](#main-menu)

[Challenge Screen	12](#challenge-screen)

[Results Screen	13](#results-screen)

[10\. Art Direction	13](#10.-art-direction)

[Visual Priorities	13](#visual-priorities)

[11\. Audio Design	13](#11.-audio-design)

[12\. Technical Design	14](#12.-technical-design)

[Implementation Priorities	14](#implementation-priorities)

[13\. Minimum Viable Product (MVP)	14](#13.-minimum-viable-product-\(mvp\))

[Required Features	14](#required-features)

[14\. Stretch Goals	15](#14.-stretch-goals)

[15\. Four-Week Development Plan	15](#15.-four-week-development-plan)

[Week 1 — Planning and Core Prototype	15](#week-1-—-planning-and-core-prototype)

[Week 2 — Prediction and Scoring	16](#week-2-—-prediction-and-scoring)

[Week 3 — Replayability and Playtesting	16](#week-3-—-replayability-and-playtesting)

[Week 4 — Refinement and Presentation	16](#week-4-—-refinement-and-presentation)

[16\. Playtesting and Evaluation	16](#16.-playtesting-and-evaluation)

[Key Evaluation Questions	16](#key-evaluation-questions)

[17\. Risks and Scope Management	17](#17.-risks-and-scope-management)

[Development Time	17](#development-time)

[Physics Accuracy	17](#physics-accuracy)

[Mathematical Accessibility	17](#mathematical-accessibility)

[Scoring Complexity	17](#scoring-complexity)

[Feature Creep	17](#feature-creep)

[18\. Open Team Decisions	17](#18.-open-team-decisions)

[19\. Project Vision	18](#19.-project-vision)

[**Theoretical Foundations and Design Applications	18**](#theoretical-foundations-and-design-applications)

[Overview	18](#overview)

[1\. Mechanics, Dynamics, and Aesthetics (MDA)	18](#1.-mechanics,-dynamics,-and-aesthetics-\(mda\))

[2\. GameFlow and Flow Theory	19](#2.-gameflow-and-flow-theory)

[3\. Self-Determination Theory	20](#3.-self-determination-theory)

[4\. Experiential Learning Theory	20](#4.-experiential-learning-theory)

[5\. Cognitive Load Theory	21](#5.-cognitive-load-theory)

[Theory-Based Playtesting and Evaluation	21](#theory-based-playtesting-and-evaluation)

[Conclusion	22](#conclusion)

[References	22](#references)

## **1\. Game Overview** {#1.-game-overview}

*Motion Commotion\!* is an educational physics puzzle game designed to introduce middle school students to foundational physics concepts through interactive experimentation, prediction, and problem-solving.

Players encounter physics-based challenges in which they adjust variables, calculate predicted outcomes, and run simulations to test their understanding.

The game emphasizes learning through experimentation rather than memorization. Students are encouraged to develop confidence with physics equations, mathematical reasoning, and algebraic manipulation in a playful, low-pressure environment.

Set in the whimsical town of Motionville on the planet Inertia, *Motion Commotion\!* challenges players to restore order after the town's Commotion Engine malfunctions, causing widespread physics-related chaos. Players apply mathematical reasoning and physics principles to solve interactive 2D puzzles, gradually helping Motionville recover from the mayhem.

### **Design Pillars** {#design-pillars}

1. **Learn by Experimenting:** Students observe how changing physical variables affects outcomes.  
2. **Predict, Test, Improve:** Students practice calculations and compare predictions against simulated results.  
3. **Accessible Learning:** Formulas, variable definitions, and hints support students without requiring prior physics expertise.  
4. **Reward Understanding:** Scoring emphasizes accuracy, improvement, and optional mastery challenges.  
5. **Play Without Punishment:** Unlimited attempts allow students to experiment without being locked out of progression.

## **2\. Narrative and Setting** {#2.-narrative-and-setting}

### **Setting** {#setting}

*Motion Commotion\!* takes place in **Motionville**, a colorful, whimsical town located on the planet **Inertia**.

Motionville is home to the **Commotion Engine**, a powerful machine designed to keep the town's systems operating smoothly. Unfortunately, the engine has malfunctioned, causing widespread physics-related mayhem throughout the town.

Everyday activities have become chaotic. Objects move at incorrect speeds, deliveries miss their destinations, and machinery behaves unpredictably.

### **Player Objective** {#player-objective}

Players take on the role of problem-solvers tasked with restoring order to Motionville.

By applying real-world physics principles, players must overcome challenges caused by the malfunctioning Commotion Engine. Each puzzle presents a different situation that requires mathematical reasoning, experimentation, and accurate predictions.

As players complete challenges, they help restore Motionville's systems and work toward resolving the Commotion Engine's malfunction.

### **Narrative Integration** {#narrative-integration}

The narrative provides context for the game's educational challenges without interrupting the core gameplay loop.

* Each level presents a physics-related problem affecting Motionville.  
* Players use calculations and simulations to determine an appropriate solution.  
* Successful completion represents progress toward restoring order.  
* More advanced challenges may represent increasingly complicated consequences of the engine's malfunction.

The game uses a lighthearted, humorous tone and colorful 2D environments. Narrative elements can be conveyed through brief introductory text, environmental details, and visual feedback rather than extensive dialogue or cutscenes.

### **Narrative Scope** {#narrative-scope}

**MVP:** Establish Motionville, the planet Inertia, and the malfunctioning Commotion Engine through a brief introduction. Connect the initial physics puzzle to a problem caused by the malfunction.

**Stretch Goals:** Introduce additional locations, recurring characters, humorous dialogue, and a final challenge involving the Commotion Engine itself.

The narrative supports the educational gameplay while remaining achievable within the four-week development schedule.

## **3\. Core Gameplay Loop** {#3.-core-gameplay-loop}

Each physics puzzle follows a consistent gameplay sequence.

**Step 1: Present the Challenge**

The player is shown level with an obstacle course and a clear objective to reach. The play is given different block functions and other utilities that assist in moving the object towards that objective. New features are introduced with an tutorial.

Example: Determine how long a ball will take to travel a specified distance.

**Step 2: Adjust Variables and Design Block-Coded Action Sequence**

The player configures relevant physical parameters through an interactive interface, such as, initial velocity, applied force on the ball, the angle at which the force is applied, or other variables appropriate to the selected physics concept.

**Step 3: Run the Simulation**

The player starts the simulation and watches the physical interaction unfold. Forces that are acting upon the ball object and its parameters are displayed. The player can view the simulation with playback features.

**Step 4: Compare Results**

The game displays the simulated result, prediction accuracy, and points earned (time-based).

**Step 5: Retry or Continue**

Players may retry the same scenario or continue to the next puzzle.

**Proposed:** A post-level mathematical challenge offers additional points for manipulating formulas or converting units.

## **4\. Gameplay and Physics Systems** {#4.-gameplay-and-physics-systems}

### **4.1 Physics Puzzles** {#4.1-physics-puzzles}

Each level focuses on a specific physics principle.

Potential topics include:

* Distance, time, and average velocity  
* Projectile motion  
* Gravity and acceleration  
* Friction and inclined surfaces  
* Momentum and collisions

**Team Decision Required:** Select the physics concept for the initial playable level.

For the four-week project, the team will prioritize one complete physics puzzle. Additional physics concepts are stretch goals.

All puzzles should use mathematically valid scenarios, with simulation behavior consistent with the equations being taught.

### **4.2 Simulation Controls** {#4.2-simulation-controls}

The game will use a 2D physics environment featuring simple, expressive objects and animations.

Players will interact primarily by configuring experiments rather than directly controlling a character.

**Planned Inputs:**

* Mouse controls for buttons, sliders, and numerical fields  
* Keyboard input for numerical predictions and interface navigation  
* A simulation start button  
* Retry and New Challenge options

**Proposed:** Additional keyboard shortcuts for common actions.

### **4.3 Formula Reference and Hints** {#4.3-formula-reference-and-hints}

Each prediction screen displays the relevant formula and defines all variables.

Example:

**Average Velocity \= Distance ÷ Time**

V\_avg \= d / t

Where:

* V\_avg \= average velocity (m/s)  
* d \= displacement (m), for a straight-line scenario  
* t \= elapsed time (s)

The formula remains visible even when students must rearrange it to solve for a different variable.

A Hint button offers assistance with identifying the unknown variable and rearranging the equation.

The intention is to teach students how to work with formulas rather than simply enter memorized answers.

## **5\. Difficulty System** {#5.-difficulty-system}

*Motion Commotion\!* features three difficulty settings that progressively increase both mathematical complexity and the precision required to complete physics puzzles.

Each difficulty uses the same underlying physics principles, objectives, and simulation systems. However, target zones become smaller at higher difficulties, requiring more accurate calculations and predictions.

### **Easy** {#easy}

* Large target zones with generous margins for error.  
* Formulas are already rearranged to isolate the required variable.  
* Students substitute provided values and calculate their answers.  
* All measurements use consistent units.

### **Medium** {#medium}

* Moderately sized target zones that require greater precision.  
* Students receive the base formula and must rearrange it to isolate the required variable.  
* All measurements use consistent units.

### **Hard** {#hard}

* Small target zones that require the greatest precision.  
* Students receive the base formula and must rearrange it to isolate the required variable.  
* Some measurements require unit conversion (e.g., centimeters to meters) before calculating.

### **Attempt-Based Scoring** {#attempt-based-scoring}

All difficulty settings use the same attempt multipliers:

* First attempt: 2× points  
* Second attempt: 1.5× points  
* Third and subsequent attempts: 1× points

Players have unlimited attempts regardless of difficulty. Retrying the same challenge advances the attempt counter, while selecting a new challenge variation resets it.

Prediction accuracy remains a separate scoring consideration, with the previously proposed accuracy multiplier system subject to team approval.

### **Development Considerations** {#development-considerations}

Difficulty settings will reuse the same level assets, physics simulation, and core mechanics wherever possible. Target sizes, numerical values, and formula presentation can be adjusted through configurable level parameters.

The MVP will prioritize one fully functional physics puzzle and its core learning loop. Additional difficulty configurations may be implemented incrementally as development time allows.

## **6\. Scoring System** {#6.-scoring-system}

Scoring rewards successful puzzle completion, accurate predictions, and optional objectives.

### **6.1 Main Puzzle Score** {#6.1-main-puzzle-score}

**Main Puzzle Score \= (Completion Points \+ Optional Objective Points) × Accuracy Multiplier × Attempt Multiplier**

The accuracy multiplier applies to the entire main puzzle score.

### **6.2 Prediction Accuracy** {#6.2-prediction-accuracy}

| Prediction Accuracy | Proposed Multiplier |
| ----- | ----- |
| 90–100% | 2.0× |
| 80–89% | 1.5× |
| 70–79% | 1.25× |
| Below 70% | 1.0× |

Students do not need a mathematically perfect prediction to receive the maximum multiplier.

**Implementation Note:** The team must define how prediction accuracy is calculated, including rounding, tolerances, and cases involving values near zero.

### **6.3 Optional Physics Objectives** {#6.3-optional-physics-objectives}

**Stretch Goal**

Levels may feature additional physical conditions that award bonus points.

Examples:

* Reach a target while traveling below a specified speed.  
* Complete a challenge within a specified time.  
* Land within a designated target zone.

Optional objectives should reinforce the physics principle rather than reward unrelated actions.

### **6.4 Post-Level Bonus Challenges** {#6.4-post-level-bonus-challenges}

**Stretch Goal**

After completing a puzzle, players may attempt an optional mathematical challenge.

Examples include:

* Rearranging the level's equation to solve for a different variable  
* Solving the same equation using different values  
* Converting meters to centimeters or seconds to minutes

These challenges award additional points after the main accuracy multiplier.

Each post-level bonus may contribute to progression only once per level to discourage repetitive point farming.

## **7\. Challenge Variations and Replayability** {#7.-challenge-variations-and-replayability}

Each physics level is designed to support three predefined numerical variations of equivalent mathematical difficulty.

For example, an average velocity puzzle could present:

| Variation | Distance | Average Velocity | Expected Time |
| ----- | ----- | ----- | ----- |
| A | 12 m | 3 m/s | 4 s |
| B | 20 m | 4 m/s | 5 s |
| C | 18 m | 3 m/s | 6 s |

These examples assume constant velocity in a straight line.

### **Retry** {#retry}

* Retains the current challenge variation.  
* Allows the student to adjust their solution.  
* Advances the attempt counter for the selected difficulty.  
* Does not limit the number of attempts.

### **New Challenge** {#new-challenge}

* Selects a different predefined variation.  
* Resets attempt-bonus eligibility for the new challenge.  
* Randomly cycles through unplayed variations before repeating them.  
* Avoids immediately repeating the previous variation.

All variations must be validated to ensure that the physics and mathematical solutions are consistent.

## **8\. Story and Player Progression** {#8.-story-and-player-progression}

### **Story Progression** {#story-progression}

*Motion Commotion\!* follows a sentient orb created by the Commotion Engine, which has malfunctioned after losing five essential physics formulas. In this world, mathematical formulas exist as physical objects, and their absence has caused widespread chaos throughout Motionville.

The orb must navigate a series of physics-based challenges to recover the missing formulas and return them to the Commotion Engine.

Each recovered formula represents a narrative milestone and progress toward restoring order to Motionville.

### **Level Progression** {#level-progression}

Players advance through physics challenges by applying mathematical reasoning, predicting outcomes, and successfully completing simulations.

Each challenge reinforces a physics concept while contributing to the orb's larger mission.

The narrative provides a sense of purpose and accomplishment without requiring extensive cutscenes, dialogue, or exploration mechanics.

### **Player Progression (Stretch Goal)** {#player-progression-(stretch-goal)}

An optional Physics Level system tracks player mastery through their highest earned scores across completed challenges.

* Only the highest score achieved for each level contributes to the player's total.  
* Improving a previous score adds only the difference.  
* One-time bonus challenges may contribute additional points.  
* Replaying challenges does not allow unlimited score farming.

Physics Level reflects demonstrated performance rather than controlling access to story content.

### **Development Scope** {#development-scope}

**MVP:** Introduce the orb's mission, establish the missing-formula premise, and connect successful puzzle completion to narrative progress.

**Stretch Goals:** Implement the recovery of all five formulas, additional narrative milestones, a completed Commotion Engine restoration sequence, and the optional Physics Level system.

## **9\. User Interface** {#9.-user-interface}

The interface should be visually friendly, readable, and approachable for middle school students.

### **Main Menu** {#main-menu}

* Play  
* Difficulty Selection  
* Instructions / How to Play

**Stretch:** Physics Level display and progress tracking.

### **Challenge Screen** {#challenge-screen}

* Current physics objective  
* Interactive 2D simulation area  
* Adjustable physical variables  
* Formula reference and variable definitions  
* Numerical prediction field  
* Hint button  
* Start Simulation button

### **Results Screen** {#results-screen}

* Predicted result  
* Actual simulation result  
* Prediction accuracy  
* Points earned  
* Retry button  
* New Challenge button  
* Continue button, when another level is available

**Stretch:** Optional objective results, Formula Flip challenges, and progression animations.

## **10\. Art Direction** {#10.-art-direction}

*Motion Commotion\!* will use a colorful 2D cartoon art style inspired by the approachable visual language of *Red Ball*.

The presentation should be playful, expressive, and easy to understand.

### **Visual Priorities** {#visual-priorities}

* Bright, contrasting colors  
* Simple geometric characters and objects  
* Clear platforms, ramps, obstacles, and targets  
* Readable numerical labels and controls  
* Simple animations that communicate motion and physical reactions  
* Visual feedback for successful predictions and completed objectives

The art should support the educational experience without overwhelming students with unnecessary visual detail.

The project will use original or appropriately licensed assets rather than directly copying existing game artwork.

**Proposed:** A friendly ball character or mascot may appear in simulations to give the game additional personality.

## **11\. Audio Design** {#11.-audio-design}

**Stretch Goal**

Potential audio features include:

* A short sound when a simulation begins  
* Collision and bouncing effects  
* Positive feedback for completing objectives  
* Distinct sounds for earning bonus points  
* Optional background music

Audio should be supportive rather than distracting.

## **12\. Technical Design** {#12.-technical-design}

**Target Platform:** Modern desktop web browsers

**Application Type:** Browser-based interactive game

**Rendering:** 2D

**Input:** Mouse and keyboard

**Technology Stack:** To Be Determined by the Team

Potential implementation approaches include JavaScript/TypeScript with HTML5 Canvas and a lightweight 2D game framework.

The team should select technology based on familiarity, physics requirements, and the four-week development schedule.

### **Implementation Priorities** {#implementation-priorities}

* Reusable physics puzzle interface  
* Data-driven numerical challenge configurations  
* Consistent prediction and results workflow  
* Reliable mathematical calculations  
* Clear separation between simulation logic and scoring logic

**Proposed:** Challenge configurations can be stored as reusable data objects, allowing the team to add new numerical variations without rebuilding the interface.

## **13\. Minimum Viable Product (MVP)** {#13.-minimum-viable-product-(mvp)}

The MVP is one complete, playable educational physics puzzle demonstrating the game's central learning experience.

### **Required Features** {#required-features}

1. One physics concept selected by the team.  
2. One functioning 2D physics simulation.  
3. Adjustable physical variables.  
4. Numerical prediction input.  
5. Visible formula reference, variable definitions, and optional hint.  
6. Simulation results compared against the student's prediction.  
7. Basic accuracy-based scoring and difficulty-based attempt bonuses.  
8. Unlimited retries.  
9. Three predefined numerical variations of the same puzzle.  
10. Simple, readable interface with mouse and keyboard support.

**Success Criterion:** A student can understand the challenge, calculate a prediction, run the simulation, interpret the results, and retry without developer assistance.

## **14\. Stretch Goals** {#14.-stretch-goals}

If the MVP is completed and tested, additional features may be implemented in the following suggested priority order.

| Priority | Feature |
| ----- | ----- |
| High | Additional physics puzzles, targeting three total |
| High | Optional physics objectives |
| Medium | Post-level formula manipulation challenges |
| Medium | Unit conversion challenges |
| Medium | Physics Level progression and personal-best tracking |
| Low | Expanded animations and audio |
| Low | Additional visual customization and polish |

The team may revise these priorities based on development progress and playtesting.

## **15\. Four-Week Development Plan** {#15.-four-week-development-plan}

### **Week 1 — Planning and Core Prototype** {#week-1-—-planning-and-core-prototype}

* Finalize the initial physics concept.  
* Select the web development framework.  
* Define the puzzle's equations and numerical variables.  
* Establish the basic interface and visual direction.  
* Create an initial working simulation.

**Milestone:** A basic physics scenario can be configured and simulated.

### **Week 2 — Prediction and Scoring** {#week-2-—-prediction-and-scoring}

* Implement numerical prediction input.  
* Display formulas, variable definitions, and hints.  
* Compare predictions with calculated or simulated outcomes.  
* Implement accuracy-based scoring.  
* Implement difficulty-based attempt bonuses.

**Milestone:** The complete prediction → simulation → feedback loop works.

### **Week 3 — Replayability and Playtesting** {#week-3-—-replayability-and-playtesting}

* Implement three validated challenge variations.  
* Add Retry and New Challenge functionality.  
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
* Do students understand how to enter predictions?  
* Can students recognize why their predictions were accurate or inaccurate?  
* Do hints support learning without giving away solutions?  
* Does the scoring system encourage experimentation?  
* Are the controls and interface intuitive?  
* Do students feel encouraged to try again after an incorrect prediction?

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

The full scoring system may be difficult to balance within the available development time. A simplified implementation should be considered if necessary.

### **Feature Creep** {#feature-creep}

Additional levels, progression systems, and cosmetic features should remain optional until the MVP is stable.

## **18\. Open Team Decisions** {#18.-open-team-decisions}

The following decisions require team discussion and approval:

1. Which physics concept will be implemented first?  
2. Which web framework and physics tools will be used?  
3. What specific physical variables can students manipulate in the first puzzle?  
4. Must students submit a prediction before running a simulation, or can they freely experiment first?  
5. How will prediction accuracy and numerical tolerances be calculated?  
6. Which proposed scoring mechanics are essential for the MVP?  
7. How will tasks be distributed across the development team?  
8. What stretch features are realistic after the MVP is complete?

## **19\. Project Vision** {#19.-project-vision}

*Motion Commotion\!* aims to make physics approachable by turning abstract equations into interactive, understandable experiences.

Rather than simply telling students how physics works, the game invites them to make predictions, experiment with variables, observe results, and develop confidence through practice.

**Our goal is not to make students memorize physics equations. It is to help them discover that they can understand and use them.**

The project will prioritize one polished, educationally meaningful gameplay experience over a larger collection of unfinished features.

# **Theoretical Foundations and Design Applications** {#theoretical-foundations-and-design-applications}

## **Overview** {#overview}

*Motion Commotion\!* incorporates established game design frameworks and research-based learning theories to support engagement, experimentation, and mathematical understanding.

The game's core systems emphasize player choice, adjustable difficulty, immediate feedback, and learning through repeated experimentation. These design decisions are informed by the following theoretical foundations.

### **1\. Mechanics, Dynamics, and Aesthetics (MDA)** {#1.-mechanics,-dynamics,-and-aesthetics-(mda)}

**Research:** Hunicke, LeBlanc, and Zubek (2004)

The MDA framework examines games through three interconnected components: mechanics, dynamics, and aesthetics. Mechanics describe the game's rules and systems, dynamics describe how players interact with those systems, and aesthetics describe the experiences those interactions are intended to produce.

**Application to Motion Commotion\!:**

* **Mechanics:** Players manipulate physics variables, predict numerical outcomes, run simulations, and earn scores based on performance.  
* **Dynamics:** Players experiment with different values, compare predictions against simulated results, and refine their calculations through repeated attempts.  
* **Aesthetics:** The intended experiences include curiosity, discovery, challenge, accomplishment, and satisfaction from overcoming obstacles.

The narrative reinforces these experiences by giving players a meaningful objective: guiding a sentient orb through Motionville to recover five missing formulas and restore the Commotion Engine.

The MDA framework helps the development team evaluate whether the implemented mechanics produce the intended player experience.

### **2\. GameFlow and Flow Theory** {#2.-gameflow-and-flow-theory}

**Research:** Sweetser and Wyeth (2005), building on Csikszentmihalyi's Flow Theory.

GameFlow identifies elements associated with player enjoyment, including clear goals, appropriate challenges, player skills, control, and feedback.

**Application to Motion Commotion\!:**

The game's difficulty system is designed to accommodate different levels of mathematical proficiency:

* **Easy:** Rearranged formulas, consistent units, and larger target zones.  
* **Medium:** Formula rearrangement, consistent units, and smaller target zones.  
* **Hard:** Formula rearrangement, unit conversions, and the smallest target zones.

Players can select an appropriate challenge without changing the fundamental physics concept being taught.

Clear objectives establish what players need to accomplish, while immediate simulation results provide feedback about their decisions.

Unlimited retries allow players to improve their understanding without being permanently penalized for mistakes.

These systems are intended to balance challenge and player ability, reducing unnecessary frustration while preserving meaningful difficulty.

### **3\. Self-Determination Theory** {#3.-self-determination-theory}

**Research:** Ryan, Rigby, and Przybylski (2006)

Self-Determination Theory identifies autonomy, competence, and relatedness as fundamental psychological needs associated with motivation. Research applying this theory to video games has connected perceived autonomy and competence with player enjoyment.

**Application to Motion Commotion\!:**

**Autonomy:** Players choose their difficulty level, experiment with variables, and decide whether to retry an existing challenge or generate a new variation.

**Competence:** Players receive feedback on prediction accuracy and can improve their scores as their mathematical understanding develops.

**Relatedness:** The narrative may encourage a sense of connection to the sentient orb and the fictional world of Motionville, although this aspect is less central to the design and would require playtesting to evaluate.

The optional Physics Level system further supports competence by recognizing improvements in personal-best performance rather than rewarding repetitive score farming.

These systems aim to make mathematical problem-solving feel self-directed and rewarding.

### **4\. Experiential Learning Theory** {#4.-experiential-learning-theory}

**Research:** Kolb (1984)

Experiential Learning Theory describes learning as a process involving concrete experience, reflective observation, abstract conceptualization, and active experimentation.

**Application to Motion Commotion\!:**

The game's prediction-and-simulation loop encourages players to engage in a similar learning process:

1. **Prediction:** Players apply physics concepts and mathematical formulas to anticipate an outcome.  
2. **Experimentation:** Players configure variables and run the simulation.  
3. **Observation:** Players compare the simulated result with their prediction.  
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
* Easy difficulty reduces mathematical complexity by presenting rearranged formulas and consistent units.  
* Medium and Hard progressively introduce algebraic manipulation and unit conversions.

These features are intended to help players concentrate on the physics concepts being taught rather than struggling with unclear instructions or excessive information.

The game should avoid presenting unnecessary visual distractions or excessive information during mathematical problem-solving.

## **Theory-Based Playtesting and Evaluation** {#theory-based-playtesting-and-evaluation}

The development team can use these theoretical foundations to evaluate whether the gameplay achieves its intended objectives.

| Framework | Evaluation Focus |
| ----- | ----- |
| MDA | Whether mechanics produce experimentation, curiosity, and satisfaction |
| GameFlow | Whether challenge levels, goals, and feedback support engagement |
| Self-Determination Theory | Whether players experience choice, competence, and motivation |
| Experiential Learning | Whether players use feedback to revise predictions and improve understanding |
| Cognitive Load Theory | Whether instructions, formulas, and interface elements are understandable without unnecessary confusion |

Playtesting may incorporate observation, questionnaires, prediction accuracy, retry patterns, and short conceptual questions.

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

