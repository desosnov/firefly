/*
 * AnimationSetPlayer.cpp
 *
 *  Created on: Aug 10, 2016
 *      Author: Denis
 */

#include <animation/AnimationSetPlayer.h>

AnimationSetPlayer::~AnimationSetPlayer() {
	// TODO Auto-generated destructor stub
}

void AnimationSetPlayer::initInternal() {
	curAnim = buildNextAnimation();
	curAnim->init(0.0);


	nextSwitch = rand(minDuration, maxDuration);
	log("[ASP] Initialized animation set player with next switch at " + to_string(nextSwitch));
}

void AnimationSetPlayer::updateInternal(double time) {
	if(subpixelSampling) {
		subpixelSampling = false;
		curAnim->toggleSubpixelSampling();
	}

	if(time > nextSwitch && !nextAnim) {
		curAnim->beginWrappingUp();
		nextSwitch += 1000.0;
		log("[ASP] Hit switch time, wrapping up cur animation");
	}

	if(!nextAnim && curAnim->readyForNextAnimation()) {
		nextAnim = buildNextAnimation();
		nextAnim->init(time);

		nextSwitch = time + rand(minDuration, maxDuration);
		log("[ASP] Cur animation is ready for next animation, starting next animation");
		log("[ASP] Next switch: " + to_string(nextSwitch));
	}

	if(curAnim->finished()) {
		curAnim = nextAnim;
		nextAnim = NULL;
		log("[ASP] Cur animation is finished, replacing with next animation");
		log("[ASP] Next switch: " + to_string(nextSwitch));
	}

	if(nextAnim) {
		nextAnim->update(time);
	}
	curAnim->update(time);
}

glm::vec4 AnimationSetPlayer::renderPixelInternal(glm::vec3 pos, ArbitraryMap* details) {
	glm::vec4 color = glm::vec4(0.0, 0.0, 0.0, 0.0);
	if(nextAnim) {
		color = nextAnim->renderPixel(pos, details);
	}

	color = blendColors(color, curAnim->renderPixel(pos, details));
	return color;
}

void AnimationSetPlayer::beginWrappingUp() {
}

bool AnimationSetPlayer::readyForNextAnimation() {
}

bool AnimationSetPlayer::finished() {
}

void AnimationSetPlayer::randomizeShaders(AAnimation* anim) {
	anim->resetShaders();
	int shaders = rand() % (ASP_MAX_SHADERS + 1);
	shaders = 1;
	printf("[ASP] Next animation has %d shaders\n", shaders);
	for(int shader = 0; shader < shaders; shader++) {
		anim->addShader(pixelShaders->next());
	}
}

AAnimation* AnimationSetPlayer::buildNextAnimation() {
	AAnimation* newAnim = animations->next();

	AColorPalette* palette = palettes->next();
	AColorScheme* scheme = colorSchemes->next();
	scheme->setPalette(palette);
	newAnim->setColorPalette(palette);
	newAnim->setColorScheme(scheme);

	randomizeShaders(newAnim);

	return newAnim;
}
