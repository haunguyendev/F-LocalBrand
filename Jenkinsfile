pipeline {

    agent any

    
    stages {

        stage('Packaging') {

            steps {
                
                sh 'docker build --pull --rm -f Dockerfile -t flocalbrandapi:latest .'
                
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

        stage('Deploy FE to DEV') {
            steps {
                withCredentials([string(credentialsId: 'SECRET_KEY', variable: 'SECRET_KEY'), string(credentialsId: 'DB_SERVER', variable: 'DB_SERVER'), string(credentialsId: 'DB_NAME', variable: 'DB_NAME'), string(credentialsId: 'DB_USER', variable: 'DB_USER'), string(credentialsId: 'DB_PASSWORD', variable: 'DB_PASSWORD'), string(credentialsId: 'DB_TRUST_SERVER_CERTIFICATE', variable: 'DB_TRUST_SERVER_CERTIFICATE'), string(credentialsId: 'DB_MULTIPLE_ACTIVE_RESULT_SETS', variable: 'DB_MULTIPLE_ACTIVE_RESULT_SETS'), string(credentialsId:"CLIENT_ID", variable:"CLIENT_ID"), string(credentialsId:"CLIENT_SECRET", variable:"CLIENT_SECRET")]) {
                    echo 'Deploying and cleaning'
                    sh 'docker container stop flocalbrandapi || echo "this container does not exist" '
                    sh 'echo y | docker system prune '
                    sh '''docker container run -e SECRET_KEY="${SECRET_KEY}" -e DB_SERVER="${DB_SERVER}" -e DB_NAME="${DB_NAME}" -e DB_USER="${DB_USER}" -e DB_PASSWORD="${DB_PASSWORD}" -e DB_TRUST_SERVER_CERTIFICATE="${DB_TRUST_SERVER_CERTIFICATE}" -e DB_MULTIPLE_ACTIVE_RESULT_SETS="${DB_MULTIPLE_ACTIVE_RESULT_SETS}" -e CLIENT_ID="${CLIENT_ID}" -e CLIENT_SECRET="${CLIENT_SECRET}" -d --rm --name flocalbrandapi -p 8082:8080 -p 8083:8081 chalsfptu/flocalbrandapi '''
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