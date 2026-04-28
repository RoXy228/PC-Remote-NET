package com.pc.pcremotenet

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent

import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.material3.Surface
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color

import com.pc.pcremotenet.data.AppPrefs
import com.pc.pcremotenet.hud.navigation.AppNavGraph
import com.pc.pcremotenet.ui.theme.PCRemoteNETTheme
import androidx.core.splashscreen.SplashScreen.Companion.installSplashScreen
class MainActivity : ComponentActivity() {

    private lateinit var prefs: AppPrefs

    override fun onCreate(savedInstanceState: Bundle?) {

        val splash = installSplashScreen()

        splash.setOnExitAnimationListener { splashView ->

            splashView.view.animate()
                .alpha(0f)
                .setDuration(200)
                .withEndAction {
                    splashView.remove()
                }
                .start()
        }

        super.onCreate(savedInstanceState)

        prefs = AppPrefs(this)

        setContent {

            PCRemoteNETTheme {

                Surface(
                    modifier = Modifier.fillMaxSize(),
                    color = Color.Black
                ) {
                    AppNavGraph(prefs)
                }

            }

        }
    }
}