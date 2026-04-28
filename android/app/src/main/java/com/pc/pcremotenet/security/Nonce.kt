package com.pc.pcremotenet.security

import java.util.UUID

fun generateNonce(): String {
    return UUID.randomUUID().toString()
}