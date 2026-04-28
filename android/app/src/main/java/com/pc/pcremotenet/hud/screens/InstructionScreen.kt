package com.pc.pcremotenet.hud.screens

import androidx.compose.runtime.Composable
import androidx.compose.runtime.*
import androidx.compose.ui.Modifier
import androidx.compose.ui.viewinterop.AndroidView
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.background
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.Alignment
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.animation.core.animateFloatAsState
import androidx.compose.animation.core.tween
import androidx.compose.ui.graphics.graphicsLayer

@Composable
fun InstructionScreen(onBack: () -> Unit) {

    var isLoading by remember { mutableStateOf(true) }

    val alpha by animateFloatAsState(
        targetValue = if (isLoading) 0f else 1f,
        animationSpec = tween(400)
    )

    Box(modifier = Modifier.fillMaxSize()) {

        AndroidView(
            factory = { context ->
                android.webkit.WebView(context).apply {
                    settings.javaScriptEnabled = true
                    setBackgroundColor(android.graphics.Color.BLACK)

                    webViewClient = object : android.webkit.WebViewClient() {

                        override fun onPageCommitVisible(
                            view: android.webkit.WebView?,
                            url: String?
                        ) {
                            isLoading = false
                        }
                    }

                    loadUrl("file:///android_asset/PcRemote.html")
                }
            },
            modifier = Modifier
                .fillMaxSize()
                .graphicsLayer { this.alpha = alpha }
        )

        if (isLoading) {
            Box(
                modifier = Modifier
                    .fillMaxSize()
                    .background(Color.Black),
                contentAlignment = Alignment.Center
            ) {
                CircularProgressIndicator(color = Color.White)
            }
        }
    }
}