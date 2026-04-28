package com.pc.pcremotenet.hud.screens

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.unit.dp
import com.pc.pcremotenet.data.AppPrefs
import android.app.Activity
import android.content.Intent
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.result.contract.ActivityResultContracts
import org.json.JSONObject
import androidx.compose.ui.platform.LocalContext
import android.Manifest
import androidx.core.net.toUri
import androidx.compose.material3.Text

@Composable
fun SettingsScreen(
    prefs: AppPrefs,
    onBack: () -> Unit,
    onOpenQr: () -> Unit,
    onOpenInstruction: () -> Unit
) {

    val externalIp by prefs.externalIp.collectAsState()
    val wolEnabled by prefs.wolEnabled.collectAsState()
    val pcName by prefs.pcName.collectAsState()
    val context = LocalContext.current
    val localIp by prefs.localIp.collectAsState()
    val useLocalIp by prefs.useLocalIp.collectAsState()

    var editedLocalIp by remember { mutableStateOf(localIp) }
    var editedIp by remember { mutableStateOf(externalIp) }
    var editedName by remember { mutableStateOf(pcName) }
    val permissionLauncher =
        rememberLauncherForActivityResult(
            ActivityResultContracts.RequestPermission()
        ) { granted ->

            if (granted) {
                onOpenQr()
            }

        }

    Column(
        modifier = Modifier
            .fillMaxSize()
            .background(Color.Black)
            .systemBarsPadding()
            .padding(24.dp),
        verticalArrangement = Arrangement.spacedBy(18.dp)
    ) {

        Text(
            text = "Настройки",
            color = Color.White,
            style = MaterialTheme.typography.headlineLarge
        )

        OutlinedButton(
            modifier = Modifier.fillMaxWidth(),
            onClick = {
                permissionLauncher.launch(Manifest.permission.CAMERA)
            }
        ) {
            Text("Привязать ПК по QR")
        }

        OutlinedTextField(
            value = editedIp,
            onValueChange = { editedIp = it },
            label = { Text("Внешний IP / DDNS") },
            modifier = Modifier.fillMaxWidth(),
            singleLine = true
        )
        if (
            !isPublicIp(editedIp) &&
            !isDomain(editedIp) &&
            editedIp.isNotBlank()
        ) {
            Text(
                text = "Это не белый IP",
                color = Color(0xFF64B5F6),
                style = MaterialTheme.typography.bodySmall,
                modifier = Modifier.padding(start = 4.dp, top = 4.dp)
            )
        }
        OutlinedTextField(
            value = editedLocalIp,
            onValueChange = { editedLocalIp = it },
            label = { Text("Локальный IP") },
            modifier = Modifier.fillMaxWidth(),
            singleLine = true
        )

        OutlinedTextField(
            value = editedName,
            onValueChange = { editedName = it },
            label = { Text("Имя ПК") },
            modifier = Modifier.fillMaxWidth(),
            singleLine = true
        )
        Row(
            modifier = Modifier.fillMaxWidth(),
            verticalAlignment = Alignment.CenterVertically
        ) {

            Text(
                text = "Использовать локальную сеть",
                color = Color.White,
                modifier = Modifier.weight(1f)
            )

            Switch(
                checked = useLocalIp,
                onCheckedChange = {
                    prefs.setUseLocalIp(it)
                }
            )
        }

        Row(
            modifier = Modifier.fillMaxWidth(),
            verticalAlignment = Alignment.CenterVertically
        ) {

            Text(
                text = "Wake-on-LAN",
                color = Color.White,
                modifier = Modifier.weight(1f)
            )

            Switch(
                checked = wolEnabled,
                onCheckedChange = {
                    prefs.setWolEnabled(it)
                }
            )
        }

        OutlinedButton(
            modifier = Modifier.fillMaxWidth(),
            onClick = {
                onOpenInstruction()
            }
        ) {
            Text("Инструкция по настройке")
        }

        Button(
            modifier = Modifier.fillMaxWidth(),
            onClick = {
                prefs.setExternalIp(editedIp)
                prefs.setLocalIp(editedLocalIp)
                prefs.setPcName(editedName)
                onBack()
            }
        ) {
            Text("Сохранить")
        }
    }
}
fun isPublicIp(ip: String): Boolean {

    if (ip.isBlank()) return false

    val parts = ip.split(".")
    if (parts.size != 4) return false

    val a = parts[0].toIntOrNull() ?: return false
    val b = parts[1].toIntOrNull() ?: return false

    if (a == 10) return false
    if (a == 127) return false
    if (a == 169 && b == 254) return false
    if (a == 192 && b == 168) return false
    if (a == 172 && b in 16..31) return false
    if (a == 100 && b in 64..127) return false

    if (ip == "0.0.0.0") return false
    if (ip == "255.255.255.255") return false

    return true
}
fun isDomain(input: String): Boolean {
    return input.contains(".") && input.any { it.isLetter() }
}