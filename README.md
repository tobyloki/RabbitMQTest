# RabbitMQ Pub/Sub

Use case scenario: a single producer sends data to a single consumer (code technically allows for multiple producers)
- One to one relationship

![RabbitMQOneToOne](./images/RabbitMQOneToOne.gif)
- Diagram made with http://tryrabbitmq.com

## Typical scenario

1. Producer generates a pin code and automatically starts sending messages to queue (even if it doesn't exist yet)
2. Consumer creates new queue with same pin code and subscribes to messages sent to this queue
    - After subscribing, no more consumers can subscribe to the same queue name as it is exclusive
    - Queue will automatically be deleted after consumer unsubscribes