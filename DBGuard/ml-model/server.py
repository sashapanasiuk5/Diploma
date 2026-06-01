import asyncio
import struct
import re
import numpy as np
import fasttext
#import uvloop

#asyncio.set_event_loop_policy(uvloop.EventLoopPolicy())

HOST = "0.0.0.0"
PORT = 8081

model = fasttext.load_model("/ml-model/sql_injection_detection.ftz")

def preprocess_data(query: str) -> str:
    query = query.lower()

    query = re.sub(r"--", " -- ", query)
    query = re.sub(r"/\*", " /* ", query)
    query = re.sub(r"\*/", " */ ", query)

    query = re.sub(r"'(?:''|[^'])*'", " VAL ", query)

    query = re.sub(r"\bnull\b", " VAL ", query)

    query = re.sub(r"\b\d+(\.\d+)?([eE][+-]?\d+)?\b", " VAL ", query)

    query = re.sub(r"([=<>!]+)", r" \1 ", query)
    query = re.sub(r"([(),;])", r" \1 ", query)

    query = re.sub(r"\s+", " ", query).strip()

    return query

async def handle_client(reader, writer):
    try:
        data = await reader.readexactly(4)

        query_length = struct.unpack("!I", data)[0]

        query_bytes = await reader.readexactly(query_length)
        query_text = query_bytes.decode()

        label, accuracy = model.predict(preprocess_data(query_text))

        response = bytearray(5)

        if label[0] == '__label__0' :
            response[0] = 0
        else:
            response[0] = 1
            print("Injection detected", flush=True)
            print("Accuracy: ", accuracy, flush=True)
            print(repr(query_text), flush=True)

        response[1:5] = struct.pack('<f', accuracy.astype(np.float32)[0])

        writer.write(response)
        await writer.drain()

    except ConnectionResetError:
        pass

    finally:
        writer.close()
        await writer.wait_closed()

async def main():
    server = await asyncio.start_server(
        handle_client,
        HOST,
        PORT,
        #reuse_port=True,
        backlog=65535
    )

    async with server:
        await server.serve_forever()

asyncio.run(main())