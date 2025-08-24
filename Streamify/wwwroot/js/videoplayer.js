// Video Player JavaScript Functions
window.initializeVideoPlayer = (videoElement) => {
    // Set initial volume
    videoElement.volume = 1.0;

    // Add keyboard shortcuts
    document.addEventListener('keydown', (e) => {
        if (document.activeElement === document.body) {
            switch(e.key) {
                case ' ':
                case 'k':
                    e.preventDefault();
                    if (videoElement.paused) {
                        videoElement.play();
                    } else {
                        videoElement.pause();
                    }
                    break;
                case 'ArrowLeft':
                    e.preventDefault();
                    videoElement.currentTime = Math.max(0, videoElement.currentTime - 10);
                    break;
                case 'ArrowRight':
                    e.preventDefault();
                    videoElement.currentTime = Math.min(videoElement.duration, videoElement.currentTime + 10);
                    break;
                case 'ArrowUp':
                    e.preventDefault();
                    videoElement.volume = Math.min(1, videoElement.volume + 0.1);
                    break;
                case 'ArrowDown':
                    e.preventDefault();
                    videoElement.volume = Math.max(0, videoElement.volume - 0.1);
                    break;
                case 'm':
                    e.preventDefault();
                    videoElement.muted = !videoElement.muted;
                    break;
                case 'f':
                    e.preventDefault();
                    toggleFullscreen(videoElement);
                    break;
            }
        }
    });
};

window.playVideo = (videoElement) => {
    return videoElement.play();
};

window.pauseVideo = (videoElement) => {
    videoElement.pause();
};

window.getCurrentTime = (videoElement) => {
    return videoElement.currentTime;
};

window.setCurrentTime = (videoElement, time) => {
    videoElement.currentTime = time;
};

window.getVideoDuration = (videoElement) => {
    return videoElement.duration || 0;
};

window.setVolume = (videoElement, volume) => {
    videoElement.volume = volume;
};

window.setPlaybackRate = (videoElement, rate) => {
    videoElement.playbackRate = rate;
};

window.toggleFullscreen = (videoElement) => {
    if (!document.fullscreenElement) {
        videoElement.parentElement.requestFullscreen().catch(err => {
            console.log('Error attempting to enable fullscreen:', err);
        });
    } else {
        document.exitFullscreen();
    }
};