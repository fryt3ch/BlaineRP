﻿const pointing =
{
    active: false,
    interval: null,
    lastSent: 0,

    start: function () {
        if (!this.active) {
            this.active = true;

            mp.game.streaming.requestAnimDict("anim@mp_point");
            while (!mp.game.streaming.hasAnimDictLoaded("anim@mp_point"))
                mp.game.wait(0);

            mp.game.invoke("0x0725a4ccfded9a70", mp.players.local.handle, 0, 1, 1, 1);
            mp.players.local.setConfigFlag(36, true)
            mp.players.local.taskMoveNetwork("task_mp_pointing", 0.5, false, "anim@mp_point", 24);
            mp.game.streaming.removeAnimDict("anim@mp_point");

            this.interval = setInterval(this.process.bind(this), 0);
        }
    },

    stop: function () {
        if (this.active) {
            clearInterval(this.interval);
            this.interval = null
            this.active = false;

            mp.game.invoke("0xd01015c7316ae176", mp.players.local.handle, "Stop");

            if (!mp.players.local.isInjured())
                mp.players.local.clearTasks();

            if (!mp.players.local.isInAnyVehicle(true))
                mp.game.invoke("0x0725a4ccfded9a70", mp.players.local.handle, 1, 1, 1, 1);

            mp.players.local.setConfigFlag(36, false);
            mp.players.local.clearTasks()
        }
    },

    gameplayCam: mp.cameras.new("gameplay"),
    lastSync: 0,

    getRelativePitch: function () {
        const camRot = this.gameplayCam.getRot(2);

        return camRot.x - mp.players.local.getPitch();
    },

    process: function () {
        if (this.active) {

            if (!mp.system.isFocused)
                this.stop();

            mp.game.invoke("0x921ce12c489c4c41", mp.players.local.handle);

            let camPitch = this.getRelativePitch();

            if (camPitch < -70.0)
                camPitch = -70.0;
            else if (camPitch > 42.0)
                camPitch = 42.0;

            camPitch = (camPitch + 70.0) / 112.0;

            let camHeading = mp.game.cam.getGameplayCamRelativeHeading();
            let cosCamHeading = mp.game.system.cos(camHeading);
            let sinCamHeading = mp.game.system.sin(camHeading);

            if (camHeading < -180.0)
                camHeading = -180.0;
            else if (camHeading > 180.0)
                camHeading = 180.0;

            camHeading = (camHeading + 180.0) / 360.0;

            let coords = mp.players.local.getOffsetFromGivenWorldCoords((cosCamHeading * -0.2) - (sinCamHeading * (0.4 * camHeading + 0.3)), (sinCamHeading * -0.2) + (cosCamHeading * (0.4 * camHeading + 0.3)), 0.6);
            let blocked = (typeof mp.raycasting.testPointToPoint(new mp.Vector3(coords.x, coords.y, coords.z - 0.2), new mp.Vector3(coords.x, coords.y, coords.z + 0.2), mp.players.local.handle, 7) !== 'undefined');

            mp.game.invoke('0xd5bb4025ae449a4e', mp.players.local.handle, "Pitch", camPitch);
            mp.game.invoke('0xd5bb4025ae449a4e', mp.players.local.handle, "Heading", camHeading * -1.0 + 1.0);
            mp.game.invoke('0xb0a6cfd2c69c1088', mp.players.local.handle, "isBlocked", blocked);
            mp.game.invoke('0xb0a6cfd2c69c1088', mp.players.local.handle, "isFirstPerson", mp.game.invoke('0xee778f8c7e1142e2', mp.game.invoke('0x19cafa3c87f7c2ff')) == 4);

            if ((Date.now() - this.lastSent) > 100) {
                this.lastSent = Date.now();

                let camDir = this.gameplayCam.getDirection();

                mp.events.callLocal("Players::FingerPointUpdate", camDir.x, camDir.y, camDir.z);
                mp.events.callRemoteUnreliable("fpsu", camPitch, camHeading);
            }
        }
    }
}

mp.events.add("fpsu", (playerH, camPitch, camHeading) => {
    const netPlayer = playerH;

    if (netPlayer != null) {
        if (netPlayer != mp.players.local) {
            netPlayer.lastReceivedPointing = Date.now();

            if (!netPlayer.pointingInterval) {
                netPlayer.pointingInterval = setInterval((function () {
                    if ((Date.now() - netPlayer.lastReceivedPointing) > 1000) {
                        clearInterval(netPlayer.pointingInterval);

                        netPlayer.lastReceivedPointing = undefined;
                        netPlayer.pointingInterval = undefined;

                        mp.game.invoke("0xd01015c7316ae176", netPlayer.handle, "Stop");


                        if (!netPlayer.isInAnyVehicle(true)) {
                            mp.game.invoke("0x0725a4ccfded9a70", netPlayer.handle, 1, 1, 1, 1);
                        }
                        netPlayer.setConfigFlag(36, false);

                    }
                }).bind(netPlayer), 500);

                mp.game.streaming.requestAnimDict("anim@mp_point");

                while (!mp.game.streaming.hasAnimDictLoaded("anim@mp_point")) {
                    mp.game.wait(0);
                }

                mp.game.invoke("0x0725a4ccfded9a70", netPlayer.handle, 0, 1, 1, 1);
                netPlayer.setConfigFlag(36, true)
                netPlayer.taskMoveNetwork("task_mp_pointing", 0.5, false, "anim@mp_point", 24);
                mp.game.streaming.removeAnimDict("anim@mp_point");
            }
            mp.game.invoke('0xd5bb4025ae449a4e', netPlayer.handle, "Pitch", camPitch);
            mp.game.invoke('0xd5bb4025ae449a4e', netPlayer.handle, "Heading", camHeading * -1.0 + 1.0);
            mp.game.invoke('0xb0a6cfd2c69c1088', netPlayer.handle, "isBlocked", 0);
            mp.game.invoke('0xb0a6cfd2c69c1088', netPlayer.handle, "isFirstPerson", 0);
        }
    }
});

mp.events.add("fpoint_toggle", (state) => {
    if (state)
        pointing.start();
    else
        pointing.stop();
});