package com.pc.pcremotenet.hud.screens

import androidx.compose.foundation.background
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.unit.dp
import com.pc.pcremotenet.data.AppPrefs

@Composable
fun HomeScreen(
    prefs: AppPrefs,
    onOpenSettings: () -> Unit,
    onOpenControl: () -> Unit
) {

    val externalIp by prefs.externalIp.collectAsState()
    val pcName by prefs.pcName.collectAsState()

    val isLinked = externalIp.isNotEmpty()

    Column(
        modifier = Modifier
            .fillMaxSize()
            .background(Color.Black)
            .padding(horizontal = 24.dp)
            .padding(top = 60.dp),
        verticalArrangement = Arrangement.spacedBy(20.dp)
    ) {

        Text(
            text = "PC REMOTE",
            color = Color.White,
            style = MaterialTheme.typography.headlineLarge
        )

        Card(
            modifier = Modifier
                .fillMaxWidth()
                .then(
                    if (isLinked)
                        Modifier.clickable { onOpenControl() }
                    else Modifier
                ),
            shape = RoundedCornerShape(16.dp),
            colors = CardDefaults.cardColors(
                containerColor =
                    if (isLinked) Color(0xFF444444)
                    else Color(0xFF2A2A2A)
            )
        ) {

            Column(
                modifier = Modifier.padding(20.dp)
            ) {

                if (!isLinked) {

                    Text(
                        text = "ПК не привязан",
                        color = Color.LightGray
                    )

                } else {

                    Text(
                        text = pcName,
                        color = Color.White
                    )

                    Text(
                        text = "Нажмите для управления",
                        color = Color.LightGray
                    )
                }
            }
        }

        OutlinedButton(
            modifier = Modifier.fillMaxWidth(),
            onClick = onOpenSettings
        ) {
            Text("Настройки")
        }
    }
}