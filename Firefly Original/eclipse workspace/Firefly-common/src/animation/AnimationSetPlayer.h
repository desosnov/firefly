/*
 * AnimationSetPlayer.h
 *
 *  Created on: Aug 10, 2016
 *      Author: Denis
 */

#ifndef ANIMATION_ANIMATIONSETPLAYER_H_
#define ANIMATION_ANIMATIONSETPLAYER_H_

#include <animation/base/AAnimation.h>
#include <animation/sets/AnimationSet.h>
#include <animation/sets/ColorPaletteSet.h>
#include <animation/sets/ColorSchemeSet.h>
#include <animation/sets/PixelShaderSet.h>
#include <glm/glm.hpp>
#include <stage/PixelStage.h>

#define ASP_MAX_SHADERS 5

class AnimationSetPlayer: public AAnimation {
private:
	AnimationSet* animations;
	AAnimation *curAnim = NULL, *nextAnim = NULL;
	PixelShaderSet* pixelShaders;
	ColorPaletteSet* palettes;
	ColorSchemeSet* colorSchemes;

	double minDuration, maxDuration, nextSwitch;

	void randomizeShaders(AAnimation* anim);

protected:
	void initInternal();
	void updateInternal(double time);
	AAnimation* buildNextAnimation();

public:
	AnimationSetPlayer(
			PixelStage* stage,
			AnimationSet* animations,
			PixelShaderSet* pixelShaders = new AllPixelShaders(),
			ColorPaletteSet* palettes = new AllColorPalettes(),
			ColorSchemeSet* colorSchemes = new AllColorSchemes(),
			double minDuration = 30.0,
			double maxDuration = 300.0)
			: AAnimation(stage),
			  minDuration(minDuration),
			  maxDuration(maxDuration),
			  animations(animations),
			  pixelShaders(pixelShaders),
			  palettes(palettes),
			  colorSchemes(colorSchemes)
	{};
	virtual ~AnimationSetPlayer();

	glm::vec4 renderPixelInternal(glm::vec3 pos, ArbitraryMap* details);

	void beginWrappingUp();
	bool readyForNextAnimation();
	bool finished();
};

#endif /* ANIMATION_ANIMATIONSETPLAYER_H_ */
