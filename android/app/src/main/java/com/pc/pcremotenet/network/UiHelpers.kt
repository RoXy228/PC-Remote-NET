package com.pc.pcremotenet.network

import android.content.Context
import android.widget.Toast

fun showResult(
    context: Context,
    response: RemoteResponse?,
    successMessage: String
) {

    if (response == null) {

        Toast.makeText(
            context,
            "Ошибка соединения",
            Toast.LENGTH_SHORT
        ).show()

    }
    else if (!response.Ok) {

        Toast.makeText(
            context,
            "Неверный ключ",
            Toast.LENGTH_LONG
        ).show()

    }
    else {

        Toast.makeText(
            context,
            successMessage,
            Toast.LENGTH_SHORT
        ).show()

    }
}