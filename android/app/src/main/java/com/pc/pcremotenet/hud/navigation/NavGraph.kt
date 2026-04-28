package com.pc.pcremotenet.hud.navigation

import androidx.compose.runtime.Composable
import androidx.navigation.compose.*
import com.pc.pcremotenet.data.AppPrefs
import com.pc.pcremotenet.hud.screens.HomeScreen
import com.pc.pcremotenet.hud.screens.SettingsScreen
import com.pc.pcremotenet.hud.screens.QrScannerScreen
import com.pc.pcremotenet.hud.screens.ControlScreen
import androidx.navigation.compose.*
import androidx.compose.animation.*
import androidx.compose.animation.core.tween
import com.pc.pcremotenet.hud.screens.InstructionScreen
@Composable
fun AppNavGraph(
    prefs: AppPrefs
) {

    val navController = rememberNavController()

    NavHost(
        navController = navController,
        startDestination = Routes.HOME,
        enterTransition = {
            slideIntoContainer(
                AnimatedContentTransitionScope.SlideDirection.Left,
                animationSpec = tween(250)
            )
        },
        exitTransition = {
            slideOutOfContainer(
                AnimatedContentTransitionScope.SlideDirection.Left,
                animationSpec = tween(250)
            )
        },
        popEnterTransition = {
            slideIntoContainer(
                AnimatedContentTransitionScope.SlideDirection.Right,
                animationSpec = tween(250)
            )
        },
        popExitTransition = {
            slideOutOfContainer(
                AnimatedContentTransitionScope.SlideDirection.Right,
                animationSpec = tween(250)
            )
        }
    ) {

        composable(Routes.HOME) {

            HomeScreen(
                prefs = prefs,
                onOpenSettings = {
                    navController.navigate(Routes.SETTINGS)
                },
                onOpenControl = {
                    navController.navigate(Routes.CONTROL)
                }
            )
        }

        composable(Routes.SETTINGS) {

            SettingsScreen(
                prefs = prefs,
                onBack = {
                    navController.popBackStack()
                },
                onOpenQr = {
                    navController.navigate(Routes.QR)
                },
                onOpenInstruction = {
                    navController.navigate("instruction")
                }
            )
        }

        composable(Routes.QR) {

            QrScannerScreen(
                prefs = prefs,
                onFinished = {
                    navController.navigate(Routes.SETTINGS) {
                        popUpTo(Routes.SETTINGS)
                        launchSingleTop = true
                    }
                }
            )

        }

        composable(Routes.CONTROL) {

            ControlScreen(
                prefs = prefs,
                onBack = {
                    navController.popBackStack()
                }
            )
        }

        composable(Routes.INSTRUCTION) {
            InstructionScreen(onBack = { navController.popBackStack() })
        }
    }
}