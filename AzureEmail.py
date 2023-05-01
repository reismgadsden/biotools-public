import sys
from azure.communication.email import EmailClient


try:
    email = sys.argv[1]
    recep = sys.argv[2]

    sub = sys.argv[3]
    body = sys.argv[4]


    client = EmailClient.from_connection_string("")

    message = {
        "content": {
            "subject": sub,
            "plainText": body
        },
        "recipients": {
            "to": [
                {
                    "address": email,
                    "displayName": recep
                }
            ]
        },
        "senderAddress": "biotools@420745f4-5948-4f77-b983-19b5ab14ec84.azurecomm.net"
    }

    poller = client.begin_send(message)

except Exception as e:
    print(e)
