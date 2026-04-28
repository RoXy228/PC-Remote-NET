package com.pc.pcremotenet.hud.screens

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Check
import androidx.compose.material.icons.filled.Close
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.unit.dp
import com.pc.pcremotenet.data.AppPrefs
import androidx.compose.foundation.layout.systemBarsPadding
import androidx.compose.runtime.collectAsState
import androidx.compose.ui.res.painterResource
import com.pc.pcremotenet.network.sendWakeOnLan
import com.pc.pcremotenet.R
import kotlinx.coroutines.delay
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import kotlinx.coroutines.launch
import com.pc.pcremotenet.network.NetworkClient
import android.widget.Toast
import androidx.compose.ui.platform.LocalContext
import com.pc.pcremotenet.network.showResult

@Composable
fun ControlScreen(
    prefs: AppPrefs,
    onBack: () -> Unit
) {

    var isSending by remember { mutableStateOf(false) }

    val externalIp by prefs.externalIp.collectAsState()
    val wolEnabled by prefs.wolEnabled.collectAsState()
    val scope = rememberCoroutineScope()
    val aesKey by prefs.aesKey.collectAsState()
    val context = LocalContext.current
    val mac = prefs.pcMac
    val localIp by prefs.localIp.collectAsState()
    val useLocalIp by prefs.useLocalIp.collectAsState()

    val activeIp =
        if (useLocalIp && localIp.isNotBlank())
            localIp
        else
            externalIp

    var minutes by remember { mutableStateOf("") }
    var pcStatus by remember { mutableStateOf("offline") }
    LaunchedEffect(activeIp, aesKey) {

        if (activeIp.isBlank() || aesKey.isBlank())
            return@LaunchedEffect


        while (true) {

            val alive = withContext(Dispatchers.IO) {

                val response = NetworkClient.send(
                    activeIp,
                    prefs.port,
                    aesKey,
                    "STATUS"
                )

                response?.Ok == true
            }

            pcStatus = if (alive) "online" else "offline"

            delay(if (alive) 5000 else 2000)
        }
    }

    Column(
        modifier = Modifier
            .fillMaxSize()
            .background(Color.Black)
            .systemBarsPadding()
            .padding(24.dp),
        verticalArrangement = Arrangement.spacedBy(16.dp)
    ) {

        Row(
            modifier = Modifier.fillMaxWidth(),
            horizontalArrangement = Arrangement.SpaceBetween,
            verticalAlignment = Alignment.CenterVertically
        ) {

            Text(
                text = "Управление ПК",
                color = Color.White,
                style = MaterialTheme.typography.headlineMedium
            )

            Row(
                verticalAlignment = Alignment.CenterVertically,
                horizontalArrangement = Arrangement.spacedBy(8.dp)
            ) {

                Box(
                    modifier = Modifier
                        .size(12.dp)
                        .background(
                            when (pcStatus) {
                                "online" -> Color(0xFF4CAF50)
                                "booting" -> Color(0xFFFFC107)
                                else -> Color(0xFFF44336)
                            },
                            CircleShape
                        )
                )

                Icon(
                    painter = painterResource(R.drawable.ic_pc_server),
                    contentDescription = null,
                    tint = Color.Unspecified,
                    modifier = Modifier.size(22.dp)
                )
            }
        }

        if (wolEnabled && mac.isNotBlank()) {

            Button(
                modifier = Modifier.fillMaxWidth(),
                enabled = !isSending,
                colors = ButtonDefaults.buttonColors(
                    containerColor = Color(0xFF009688)
                ),
                onClick = {

                    isSending = true

                    if (useLocalIp) {

                        sendWakeOnLan(
                            mac = mac,
                            host = localIp,
                            port = 40000,
                            broadcastMode = true
                        )

                    } else {

                        sendWakeOnLan(
                            mac = mac,
                            host = externalIp,
                            port = 40000,
                            broadcastMode = false
                        )

                    }

                    isSending = false
                }
            ) {
                Text("Включить ПК", color = Color.Black)
            }

        } else if (wolEnabled && mac.isBlank()) {

            Text(
                text = "Сначала привяжите ПК через QR",
                color = Color.Gray
            )
        }

        Button(
            modifier = Modifier.fillMaxWidth(),
            colors = ButtonDefaults.buttonColors(containerColor = Color(0xFF4CAF50)),
            onClick = {

                scope.launch {

                    val response = withContext(Dispatchers.IO) {
                        NetworkClient.send(
                            activeIp,
                            prefs.port,
                            aesKey,
                            "LOCK"
                        )
                    }

                    showResult(context, response, "ПК заблокирован")
                }
            }
        ) {
            Text("Заблокировать", color = Color.Black)
        }

        Button(
            modifier = Modifier.fillMaxWidth(),
            colors = ButtonDefaults.buttonColors(containerColor = Color(0xFF8BC34A)),
            onClick = {

                scope.launch {

                    val response = withContext(Dispatchers.IO) {
                        NetworkClient.send(
                            activeIp,
                            prefs.port,
                            aesKey,
                            "SCREEN_OFF"
                        )
                    }

                    showResult(context, response, "Монитор выключен")
                }
            }
        ) {
            Text("Выключить монитор", color = Color.Black)
        }

        Button(
            modifier = Modifier.fillMaxWidth(),
            colors = ButtonDefaults.buttonColors(containerColor = Color(0xFF2196F3)),
            onClick = {

                scope.launch {

                    val response = withContext(Dispatchers.IO) {
                        NetworkClient.send(
                            activeIp,
                            prefs.port,
                            aesKey,
                            "REBOOT"
                        )
                    }

                    showResult(context, response, "ПК перезагружается")
                }
            }
        ) {
            Text("Перезагрузка", color = Color.White)
        }

        Button(
            modifier = Modifier.fillMaxWidth(),
            colors = ButtonDefaults.buttonColors(containerColor = Color(0xFFF44336)),
            onClick = {

                scope.launch {

                    val response = withContext(Dispatchers.IO) {
                        NetworkClient.send(
                            activeIp,
                            prefs.port,
                            aesKey,
                            "SHUTDOWN"
                        )
                    }

                    showResult(context, response, "ПК выключается")
                }
            }
        ) {
            Text("Выключить", color = Color.White)
        }

        Spacer(modifier = Modifier.height(5.dp))

        Text(
            text = "Таймер",
            color = Color.White
        )

        Row(
            horizontalArrangement = Arrangement.spacedBy(5.dp),
            verticalAlignment = Alignment.CenterVertically
        ) {

            OutlinedTextField(
                value = minutes,
                onValueChange = {
                    minutes = it
                },
                label = { Text("Минуты") },
                modifier = Modifier.weight(1f),
                singleLine = true
            )

            IconButton(
                modifier = Modifier
                    .size(48.dp)
                    .background(Color.Red, CircleShape),
                onClick = {

                    val m = minutes.toIntOrNull() ?: return@IconButton

                    scope.launch {

                        val response = withContext(Dispatchers.IO) {
                            NetworkClient.send(
                                activeIp,
                                prefs.port,
                                aesKey,
                                "SHUTDOWN:$m"
                            )
                        }

                        showResult(context, response, "Таймер установлен")
                    }
                }
            ) {
                Icon(Icons.Default.Check, null, tint = Color.White)
            }

            IconButton(
                modifier = Modifier
                    .size(48.dp)
                    .background(Color.DarkGray, CircleShape),
                onClick = {

                    scope.launch {

                        val response = withContext(Dispatchers.IO) {
                            NetworkClient.send(
                                activeIp,
                                prefs.port,
                                aesKey,
                                "CANCEL"
                            )
                        }

                        showResult(context, response, "Таймер отменён")
                    }
                }
            ) {
                Icon(Icons.Default.Close, null, tint = Color.White)
            }
        }

        Spacer(modifier = Modifier.height(16.dp))

        OutlinedButton(
            modifier = Modifier.fillMaxWidth(),
            onClick = onBack
        ) {
            Text("← Назад")
        }
    }
}