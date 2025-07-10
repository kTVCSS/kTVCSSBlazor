const gameArea = document.getElementById('game-area');
const scoreElem = document.getElementById('score');
const areaW = 1280, areaH = 530;
let score = 0;

function randomInt(min, max) {
    return Math.floor(Math.random() * (max - min + 1)) + min;
}

function spawnBall() {
    let ball = document.createElement('div');
    let radius = randomInt(5, 20);
    let x = randomInt(0, areaW - radius * 2);
    let y = randomInt(0, areaH - radius * 2);
    ball.classList.add('ball');
    ball.style.width = ball.style.height = radius * 2 + 'px';
    ball.style.left = x + 'px';
    ball.style.top = y + 'px';
    ball.style.background = `radial-gradient(circle at ${randomInt(25, 75)}% ${randomInt(25, 75)}%, #${Math.floor(Math.random() * 0xffffff).toString(16).padStart(6, '0')} 0%, #333 100%)`;

    ball.onclick = function () {
        ball.remove();
        score++;
        scoreElem.textContent = `Очки: ${score}`;
        spawnBall();
    };

    gameArea.appendChild(ball);
}

// Инициализация игры
spawnBall();