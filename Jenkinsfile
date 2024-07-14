pipeline {

    agent any

    
    stages {

        stage('Packaging') {

            steps {
                
                sh 'docker build --pull -f Dockerfile -t flocalbrandapi:latest .'
                
            }
        }

        stage('Push to DockerHub') {

            steps {
                withDockerRegistry(credentialsId: 'dockerhub', url: 'https://index.docker.io/v1/') {
                    sh 'docker tag flocalbrandapi:latest chalsfptu/flocalbrandapi:latest'
                    sh 'docker push chalsfptu/flocalbrandapi:latest'
                }
            }
        }

        stage('Deploy BE to DEV') {
    steps {
        withCredentials([
            string(credentialsId: 'SECRET_KEY', variable: 'SECRET_KEY'), 
            string(credentialsId: 'DB_SERVER', variable: 'DB_SERVER'), 
            string(credentialsId: 'DB_NAME', variable: 'DB_NAME'), 
            string(credentialsId: 'DB_USER', variable: 'DB_USER'), 
            string(credentialsId: 'DB_PASSWORD', variable: 'DB_PASSWORD'), 
            string(credentialsId: 'DB_TRUST_SERVER_CERTIFICATE', variable: 'DB_TRUST_SERVER_CERTIFICATE'), 
            string(credentialsId: 'DB_MULTIPLE_ACTIVE_RESULT_SETS', variable: 'DB_MULTIPLE_ACTIVE_RESULT_SETS'), 
            string(credentialsId: 'CLIENT_ID', variable: 'CLIENT_ID'), 
            string(credentialsId: 'CLIENT_SECRET', variable: 'CLIENT_SECRET'),
            string(credentialsId: 'MailSettings__Server', variable: 'MailSettings__Server'),
            string(credentialsId: 'MailSettings__Port', variable: 'MailSettings__Port'),
            string(credentialsId: 'MailSettings__SenderName', variable: 'MailSettings__SenderName'),
            string(credentialsId: 'MailSettings__SenderEmail', variable: 'MailSettings__SenderEmail'),
            string(credentialsId: 'MailSettings__UserName', variable: 'MailSettings__UserName'),
            string(credentialsId: 'MailSettings__PassWord', variable: 'MailSettings__PassWord'),
            string(credentialsId: 'CLOUDMESSAGE_TYPE', variable: 'CLOUDMESSAGE_TYPE'),
            string(credentialsId: 'CLOUDMESSAGE_PROJECT_ID', variable: 'CLOUDMESSAGE_PROJECT_ID'),
            string(credentialsId: 'CLOUDMESSAGE_PRIVATE_KEY_ID', variable: 'CLOUDMESSAGE_PRIVATE_KEY_ID'),
            string(credentialsId: 'CLOUDMESSAGE_PRIVATE_KEY', variable: 'CLOUDMESSAGE_PRIVATE_KEY'),
            string(credentialsId: 'CLOUDMESSAGE_CLIENT_EMAIL', variable: 'CLOUDMESSAGE_CLIENT_EMAIL'),
            string(credentialsId: 'CLOUDMESSAGE_CLIENT_ID', variable: 'CLOUDMESSAGE_CLIENT_ID'),
            string(credentialsId: 'CLOUDMESSAGE_AUTH_URI', variable: 'CLOUDMESSAGE_AUTH_URI'),
            string(credentialsId: 'CLOUDMESSAGE_TOKEN_URI', variable: 'CLOUDMESSAGE_TOKEN_URI'),
            string(credentialsId: 'CLOUDMESSAGE_AUTH_PROVIDER_X509_CERT_URL', variable: 'CLOUDMESSAGE_AUTH_PROVIDER_X509_CERT_URL'),
            string(credentialsId: 'CLOUDMESSAGE_CLIENT_X509_CERT_URL', variable: 'CLOUDMESSAGE_CLIENT_X509_CERT_URL'),
            string(credentialsId: 'CLOUDMESSAGE_UNIVERSE_DOMAIN', variable: 'CLOUDMESSAGE_UNIVERSE_DOMAIN'),
            string(credentialsId: 'FIREBASE_API_KEY', variable: 'FIREBASE_API_KEY'),
            string(credentialsId: 'FIREBASE_AUTH_EMAIL', variable: 'FIREBASE_AUTH_EMAIL'),
            string(credentialsId: 'FIREBASE_AUTH_PASSWORD', variable: 'FIREBASE_AUTH_PASSWORD'),
            string(credentialsId: 'FIREBASE_BUCKET', variable: 'FIREBASE_BUCKET'),
            string(credentialsId: 'VNPaySettings__ReturnUrl', variable: 'VNPaySettings__ReturnUrl'),
            string(credentialsId: 'VNPaySettings__PaymentUrl', variable: 'VNPaySettings__PaymentUrl'),
            string(credentialsId: 'VNPaySettings__TmnCode', variable: 'VNPaySettings__TmnCode'),
            string(credentialsId: 'VNPaySettings__HashSecret', variable: 'VNPaySettings__HashSecret'),
            string(credentialsId: 'VNPaySettings__Version', variable: 'VNPaySettings__Version')
        ]) {
            echo 'Deploying and cleaning'
            sh 'docker container stop flocalbrandapi || echo "this container does not exist"'
            sh 'echo y | docker system prune'
            sh '''docker container run \
                -e SECRET_KEY="${SECRET_KEY}" \
                -e DB_SERVER="${DB_SERVER}" \
                -e DB_NAME="${DB_NAME}" \
                -e DB_USER="${DB_USER}" \
                -e DB_PASSWORD="${DB_PASSWORD}" \
                -e DB_TRUST_SERVER_CERTIFICATE="${DB_TRUST_SERVER_CERTIFICATE}" \
                -e DB_MULTIPLE_ACTIVE_RESULT_SETS="${DB_MULTIPLE_ACTIVE_RESULT_SETS}" \
                -e CLIENT_ID="${CLIENT_ID}" \
                -e CLIENT_SECRET="${CLIENT_SECRET}" \
                -e MailSettings__Server="${MailSettings__Server}" \
                -e MailSettings__Port="${MailSettings__Port}" \
                -e MailSettings__SenderName="${MailSettings__SenderName}" \
                -e MailSettings__SenderEmail="${MailSettings__SenderEmail}" \
                -e MailSettings__UserName="${MailSettings__UserName}" \
                -e MailSettings__PassWord="${MailSettings__PassWord}" \
                -e CLOUDMESSAGE_TYPE="${CLOUDMESSAGE_TYPE}" \
                -e CLOUDMESSAGE_PROJECT_ID="${CLOUDMESSAGE_PROJECT_ID}" \
                -e CLOUDMESSAGE_PRIVATE_KEY_ID="${CLOUDMESSAGE_PRIVATE_KEY_ID}" \
                -e CLOUDMESSAGE_PRIVATE_KEY="${CLOUDMESSAGE_PRIVATE_KEY}" \
                -e CLOUDMESSAGE_CLIENT_EMAIL="${CLOUDMESSAGE_CLIENT_EMAIL}" \
                -e CLOUDMESSAGE_CLIENT_ID="${CLOUDMESSAGE_CLIENT_ID}" \
                -e CLOUDMESSAGE_AUTH_URI="${CLOUDMESSAGE_AUTH_URI}" \
                -e CLOUDMESSAGE_TOKEN_URI="${CLOUDMESSAGE_TOKEN_URI}" \
                -e CLOUDMESSAGE_AUTH_PROVIDER_X509_CERT_URL="${CLOUDMESSAGE_AUTH_PROVIDER_X509_CERT_URL}" \
                -e CLOUDMESSAGE_CLIENT_X509_CERT_URL="${CLOUDMESSAGE_CLIENT_X509_CERT_URL}" \
                -e CLOUDMESSAGE_UNIVERSE_DOMAIN="${CLOUDMESSAGE_UNIVERSE_DOMAIN}" \
                -e FIREBASE_API_KEY="${FIREBASE_API_KEY}" \
                -e FIREBASE_AUTH_EMAIL="${FIREBASE_AUTH_EMAIL}" \
                -e FIREBASE_AUTH_PASSWORD="${FIREBASE_AUTH_PASSWORD}" \
                -e FIREBASE_BUCKET="${FIREBASE_BUCKET}" \
                -e VNPaySettings__ReturnUrl="${VNPaySettings__ReturnUrl}" \
                -e VNPaySettings__PaymentUrl="${VNPaySettings__PaymentUrl}" \
                -e VNPaySettings__TmnCode="${VNPaySettings__TmnCode}" \
                -e VNPaySettings__HashSecret="${VNPaySettings__HashSecret}" \
                -e VNPaySettings__Version="${VNPaySettings__Version}" \
                -d --name flocalbrandapi -p 8082:8080 -p 8083:8081 chalsfptu/flocalbrandapi'''
        }
    }
}

        
 
    }
    post {
        always {
            cleanWs()
        }
    }
}