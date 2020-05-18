# Pocket Dice Game

# WebGL playable Link: http://ajfolio.orgfree.com/pocketdice/

## IDE: Visual Studio 2019, Community Edition, Unity Version: 2019.2.14f

## Gameplay Considerations:
*	Player exit scenario is only applied for Human player, not for AI bots.
*	Game over instructions were does not mention if AI players can exit mid game, hence Player exit implemented.

## Readability:
*	Ternary Operator [ ? ] not used to improve readability.

## Players:
*	“Maniac” is an Aggressive player, he will usually bet on higher amounts viz. 50-100
*	“Scrooge” is a Conservative player, he will usually bet on lower amounts viz. 10-20
*	Both AI behavior applies to their bet increments too.

## Art considerations:
*	Use of complementary color scheme: Red / Green
*	Variable names on scripts align with variable names in Scene.
*	UI Elements are always anchored to the closest screen point or within an encompassing Panel element.
*	Mip-map’s are generated for all used Sprite Elements.

## Debug Notes/Considerations:
*	Exception made to design for “Debug panel” for forced dice moves, it will not simulate a physical dice, just push the required dice number to current play.
*	Whenever debug is on, Dice and Debug panel should sync on color to make the user realize over-ride of value
*	Some Debug statements are left intentionally to show flow of gameplay.

