package com.pc.pcremotenet.hud.screens

import android.Manifest
import android.annotation.SuppressLint
import android.content.pm.PackageManager
import androidx.camera.core.*
import androidx.camera.lifecycle.ProcessCameraProvider
import androidx.camera.view.PreviewView
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.runtime.*
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.viewinterop.AndroidView
import androidx.core.content.ContextCompat
import androidx.lifecycle.compose.LocalLifecycleOwner
import com.google.mlkit.vision.barcode.BarcodeScanning
import com.google.mlkit.vision.common.InputImage
import com.pc.pcremotenet.data.AppPrefs
import org.json.JSONObject
import java.util.concurrent.Executors
import android.widget.Toast

@SuppressLint("UnsafeOptInUsageError")
@Composable
fun QrScannerScreen(
    prefs: AppPrefs,
    onFinished: () -> Unit
) {

    val context = LocalContext.current
    val lifecycleOwner = LocalLifecycleOwner.current

    val executor = remember { Executors.newSingleThreadExecutor() }

    val hasCameraPermission =
        ContextCompat.checkSelfPermission(
            context,
            Manifest.permission.CAMERA
        ) == PackageManager.PERMISSION_GRANTED

    if (!hasCameraPermission) {
        return
    }

    AndroidView(
        modifier = Modifier.fillMaxSize(),
        factory = { ctx ->

            val previewView = PreviewView(ctx)

            val cameraProviderFuture =
                ProcessCameraProvider.getInstance(ctx)

            cameraProviderFuture.addListener({

                val cameraProvider = cameraProviderFuture.get()

                val preview = Preview.Builder().build().also {
                    it.setSurfaceProvider(previewView.surfaceProvider)
                }

                val scanner = BarcodeScanning.getClient()

                val analysis = ImageAnalysis.Builder()
                    .setBackpressureStrategy(
                        ImageAnalysis.STRATEGY_KEEP_ONLY_LATEST
                    )
                    .build()

                analysis.setAnalyzer(executor) { imageProxy ->

                    val mediaImage = imageProxy.image

                    if (mediaImage != null) {

                        val image = InputImage.fromMediaImage(
                            mediaImage,
                            imageProxy.imageInfo.rotationDegrees
                        )

                        scanner.process(image)
                            .addOnSuccessListener { barcodes ->

                                for (barcode in barcodes) {

                                    val raw = barcode.rawValue ?: continue

                                    try {

                                        val obj = JSONObject(raw)

                                        val externalIp = obj.getString("external_ip")
                                        val localIp = obj.getString("local_ip")
                                        val port = obj.getInt("port")
                                        val key = obj.getString("key")
                                        val mac = obj.getString("mac")

                                        prefs.setExternalIp(externalIp)
                                        prefs.setLocalIp(localIp)
                                        prefs.setAesKey(key)
                                        prefs.port = port
                                        prefs.pcMac = mac
                                        prefs.setWolEnabled(true)

                                        Toast.makeText(
                                            context,
                                            "ПК успешно привязан",
                                            Toast.LENGTH_LONG
                                        ).show()

                                        onFinished()

                                    } catch (_: Exception) {}

                                }

                            }
                            .addOnCompleteListener {
                                imageProxy.close()
                            }

                    } else {
                        imageProxy.close()
                    }

                }

                val cameraSelector =
                    CameraSelector.DEFAULT_BACK_CAMERA

                cameraProvider.unbindAll()

                cameraProvider.bindToLifecycle(
                    lifecycleOwner,
                    cameraSelector,
                    preview,
                    analysis
                )

            }, ContextCompat.getMainExecutor(ctx))

            previewView
        }
    )
}