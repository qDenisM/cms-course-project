var dialogActive = false;
var recognizer = null;

function dialog_window() {
  var dialogSection = document.createElement("div");
  dialogSection.id = "dialogWindow";
  dialogSection.className = "dialog-window";
  document.getElementById("dialogContent").appendChild(dialogSection);

  var animationBox = document.createElement("div");
  animationBox.id = "dialogAnimation";
  animationBox.className = "dialog-animation";
  dialogSection.appendChild(animationBox);

  var titleBox = document.createElement("div");
  titleBox.className = "dialog-title";
  titleBox.innerHTML = "Диалог с базой знаний — Паровая машина";
  dialogSection.appendChild(titleBox);

  var chatBox = document.createElement("div");
  chatBox.id = "dialogChat";
  chatBox.className = "dialog-chat";
  chatBox.innerHTML =
    '<div class="bot-message">Здравствуйте! Задайте вопрос о паровой машине. ' +
    "Например: «Что такое паровая машина?», «Как работает установка?», «Как выглядит маховик?», «Кто такой Джеймс Уатт?»</div>";
  dialogSection.appendChild(chatBox);

  var formBox = document.createElement("div");
  formBox.className = "dialog-form";
  formBox.innerHTML =
    '<input type="text" id="questionInput" placeholder="Введите ваш вопрос..." autocomplete="off" />' +
    '<button type="button" id="askButton" onclick="ask()">Спросить</button>' +
    '<button type="button" id="voiceButton" title="Голосовой ввод">🎤</button>';
  dialogSection.appendChild(formBox);

  document.getElementById("questionInput").addEventListener("keydown", function (event) {
    if (event.key === "Enter") {
      ask();
    }
  });

  document.getElementById("voiceButton").addEventListener("click", function () {
    startVoiceInput();
  });

  if (typeof ya !== "undefined" && ya.speechkit) {
    try {
      recognizer = ya.speechkit.createRecognizer({
        lang: "ru-RU",
        continuous: false,
        interimResults: false
      });
      recognizer.on("result", function (text) {
        document.getElementById("questionInput").value = text;
      });
      recognizer.on("error", function () {
        appendBotMessage("Не удалось распознать речь. Введите вопрос вручную.");
      });
    } catch (e) {
      recognizer = null;
    }
  }
}

function openDialog() {
  var dialog = document.getElementById("dialogWindow");
  if (!dialog) return;
  if (dialog.classList.contains("dialog-open")) {
    dialog.classList.remove("dialog-open");
  } else {
    dialog.classList.add("dialog-open");
    $("#dialogAnimation").animate({ opacity: 1 }, 300);
  }
}

function startVoiceInput() {
  if (recognizer) {
    recognizer.start();
    appendBotMessage("Слушаю... Произнесите вопрос.");
    return;
  }
  if (window.SpeechRecognition || window.webkitSpeechRecognition) {
    var SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
    var recognition = new SpeechRecognition();
    recognition.lang = "ru-RU";
    recognition.onresult = function (event) {
      document.getElementById("questionInput").value = event.results[0][0].transcript;
    };
    recognition.onerror = function () {
      appendBotMessage("Голосовой ввод недоступен. Введите вопрос вручную.");
    };
    recognition.start();
    appendBotMessage("Слушаю... Произнесите вопрос.");
    return;
  }
  appendBotMessage("Голосовой ввод недоступен в этом браузере.");
}

function appendUserMessage(text) {
  var chat = document.getElementById("dialogChat");
  var block = document.createElement("div");
  block.className = "user-message";
  block.textContent = text;
  chat.appendChild(block);
  chat.scrollTop = chat.scrollHeight;
}

function appendBotMessage(content, isHtml) {
  var chat = document.getElementById("dialogChat");
  var block = document.createElement("div");
  block.className = "bot-message";
  if (isHtml) {
    block.innerHTML = content;
  } else {
    block.textContent = content;
  }
  chat.appendChild(block);
  chat.scrollTop = chat.scrollHeight;
}

function ask() {
  var input = document.getElementById("questionInput");
  var question = input.value.trim();
  if (!question) return;

  dialogActive = true;
  $("#dialogWindow").animate({ opacity: 1 }, 200);

  appendUserMessage(question);

  var answer = getAnswer(question);
  var isRich = typeof answer === "object" && (answer.type === "video" || answer.type === "media");

  if (isRich) {
    appendBotMessage(answer.html, true);
  } else {
    appendBotMessage(answer);
  }

  input.value = "";
  input.focus();
}

var endings = [
  ["ет", "(ет|ут|ют)"], ["ут", "(ет|ут|ют)"], ["ют", "(ет|ут|ют)"],
  ["ит", "(ит|ат|ят)"], ["ат", "(ит|ат|ят)"], ["ят", "(ит|ат|ят)"],
  ["ется", "(ет|ут|ют)ся"], ["утся", "(ет|ут|ют)ся"], ["ются", "(ет|ут|ют)ся"],
  ["ится", "(ит|ат|ят)ся"], ["атся", "(ит|ат|ят)ся"], ["ятся", "(ит|ат|ят)ся"],
  ["ал", "(ал|ала|ало|али|ал)"], ["ала", "(ал|ала|ало|али|ал)"], ["ало", "(ал|ала|ало|али|ал)"], ["али", "(ал|ала|ало|али|ал)"],
  ["ел", "(ел|ела|ело|ели|ел)"], ["ела", "(ел|ела|ело|ели|ел)"], ["ело", "(ел|ела|ело|ели|ел)"], ["ели", "(ел|ела|ело|ели|ел)"],
  ["ил", "(ил|ила|ило|или|ил)"], ["ила", "(ил|ила|ило|или|ил)"], ["ило", "(ил|ила|ило|или|ил)"], ["или", "(ил|ила|ило|или|ил)"],
  ["ен", "ен"], ["ена", "ена"], ["ено", "ено"], ["ены", "ены"],
  ["ан", "ан"], ["ана", "ана"], ["ано", "ано"], ["аны", "аны"],
  ["жен", "жен"], ["жна", "жна"], ["жно", "жно"], ["жны", "жны"]
];

function getEnding(word) {
  for (var j = 0; j < endings.length; j++) {
    if (word.substring(word.length - endings[j][0].length) === endings[j][0]) {
      return j;
    }
  }
  return -1;
}

function small(str) {
  if (!str || !str.length) return "";
  return str.substring(0, 1).toLowerCase() + str.substring(1);
}

function big(str) {
  if (!str || !str.length) return "";
  return str.substring(0, 1).toUpperCase() + str.substring(1);
}

function rowsToAnswer(rows) {
  var answer = "";
  for (var i = 0; i < rows.length; i++) {
    answer += formatRow(rows[i]);
  }
  return answer;
}

function formatRow(row) {
  if (row[1] === "— это") {
    return big(row[0] + " — это " + row[2] + ". ");
  }
  return big(row[0] + " " + row[1] + " " + row[2] + ". ");
}

function isServiceRow(row) {
  var subject = row[0].toLowerCase();
  return /^(вопрос про|поиск ответа|регулярное выражение|база знаний|каждая триада|диалоговая система|точечный ответ)/.test(subject);
}

function isVideoQuestion(question) {
  return (
    /(как|каким образом).*(работает|работают|действует|функционирует)/.test(question) &&
    /(паров|установк|машин)/.test(question)
  ) || (
    /(принцип).*(работ|действ)/.test(question) &&
    /(паров|установк|машин)/.test(question)
  ) || (
    /(опишите|расскажите|объясните).*(принцип|работ|устройств)/.test(question) &&
    /(паров|установк|машин)/.test(question)
  ) || (
    /(как устроена|как устроен|как устроено).*(паров|установк|машин)/.test(question)
  );
}

function getVideoAnswer() {
  return {
    type: "video",
    html:
      "<p><strong>Принцип работы паровой машины</strong> — смотрите видео:</p>" +
      '<div class="chat-video-wrap">' +
      '<video controls preload="metadata" playsinline width="100%">' +
      '<source src="assets/video/kursach0001-1000.mp4" type="video/mp4" />' +
      "Ваш браузер не поддерживает воспроизведение видео." +
      "</video></div>" +
      "<p>Пар поступает в цилиндр, давление перемещает поршень, шатун вращает кривошип и маховик, " +
      "а распределительный механизм переключает подачу и выпуск пара.</p>"
  };
}

var imageMap = [
  { keys: ["джеймс уатт", "james watt", "уатт"], image: "assets/images/watt.jpg", alt: "Джеймс Уатт" },
  { keys: ["томас ньюкомен", "ньюкомен"], image: "assets/images/newcomen.jpg", alt: "Томас Ньюкомен" },
  { keys: ["томас севери", "севери"], image: "assets/images/severi.jpg", alt: "Томас Севери" },
  { keys: ["джордж стефенсон", "стефенсон"], image: "assets/images/smoke_transport.jpg", alt: "Джордж Стефенсон" },
  { keys: ["роберт фултон", "фултон"], image: "assets/images/smoke_transport.jpg", alt: "Роберт Фултон" },
  { keys: ["паровая машина", "паровая установка", "паровой двигатель"], image: "assets/images/steam-engine.jpg", alt: "Паровая машина" },
  { keys: ["ранняя паровая машина", "первая паровая машина", "машина ньюкомена"], image: "assets/images/first_machine.jpg", alt: "Ранняя паровая машина" },
  { keys: ["цилиндр"], image: "assets/images/cylinder.jpg", alt: "Цилиндр" },
  { keys: ["поршень"], image: "assets/images/piston.jpg", alt: "Поршень" },
  { keys: ["шатун"], image: "assets/images/stick.jpg", alt: "Шатун" },
  { keys: ["кривошип"], image: "assets/images/crank.jpg", alt: "Кривошип" },
  { keys: ["маховик"], image: "assets/images/flywheel.jpg", alt: "Маховик" },
  { keys: ["клапанный механизм", "клапан", "золотник", "распределительный механизм"], image: "assets/images/valve-machine.jpg", alt: "Клапанный механизм" },
  { keys: ["паровоз", "локомотив"], image: "assets/images/smoke_transport.jpg", alt: "Паровоз" },
  { keys: ["промышленность", "фабрика", "завод"], image: "assets/images/smoke_industry,jpg.jpg", alt: "Паровая машина в промышленности" },
  { keys: ["современное применение", "учебный стенд"], image: "assets/images/today.jpg", alt: "Современное применение паровых машин" },
  { keys: ["промышленная революция", "цивилизация"], image: "assets/images/machine_factory.jpg", alt: "Влияние паровой машины на цивилизацию" }
];

function isVisualQuestion(question) {
  return (
    /как\s+выгляд\S*/.test(question) ||
    /кто\s+(?:такой|такая|такое|такие)/.test(question)
  );
}

function getVisualTerm(question) {
  var lookMatch = question.match(/как\s+выгляд\S*\s+(.+)$/);
  if (lookMatch) return lookMatch[1].replace(/[?!.,;:]+$/, "").trim();

  var whoMatch = question.match(/кто\s+(?:такой|такая|такое|такие)\s+(.+)$/);
  if (whoMatch) return whoMatch[1].replace(/[?!.,;:]+$/, "").trim();

  return "";
}

function findImageForTerm(term) {
  if (!term) return null;

  var normalized = term.toLowerCase().replace(/\s+/g, " ").trim();
  var bestEntry = null;
  var bestLength = 0;

  for (var i = 0; i < imageMap.length; i++) {
    for (var j = 0; j < imageMap[i].keys.length; j++) {
      var key = imageMap[i].keys[j];
      if (
        (normalized === key || normalized.indexOf(key) !== -1 || key.indexOf(normalized) !== -1) &&
        key.length > bestLength
      ) {
        bestLength = key.length;
        bestEntry = imageMap[i];
      }
    }
  }

  return bestEntry;
}

function getTextForVisualTerm(term, knowledge) {
  var normalized = term.toLowerCase().replace(/\s+/g, " ").trim();
  if (!normalized) return "";

  for (var i = 0; i < knowledge.length; i++) {
    if (isServiceRow(knowledge[i])) continue;
    if (knowledge[i][1] === "— это" && knowledge[i][0].toLowerCase() === normalized) {
      return formatRow(knowledge[i]).trim();
    }
  }

  for (var j = 0; j < knowledge.length; j++) {
    if (isServiceRow(knowledge[j])) continue;
    if (knowledge[j][1] === "— это" && knowledge[j][0].toLowerCase().indexOf(normalized) !== -1) {
      return formatRow(knowledge[j]).trim();
    }
  }

  for (var k = 0; k < knowledge.length; k++) {
    if (isServiceRow(knowledge[k])) continue;
    var subject = knowledge[k][0].toLowerCase();
    var object = knowledge[k][2].toLowerCase();
    if (
      (subject.indexOf(normalized) !== -1 || object.indexOf(normalized) !== -1) &&
      (knowledge[k][1] === "— это" || /является/.test(knowledge[k][1]))
    ) {
      return formatRow(knowledge[k]).trim();
    }
  }

  var words = normalized.split(" ");
  return fallbackBySubject(words, knowledge, "что такое " + normalized);
}

function getImageAnswer(question, knowledge) {
  if (!isVisualQuestion(question)) return null;

  var term = getVisualTerm(question);
  var imageEntry = findImageForTerm(term);
  if (!imageEntry) return null;

  var text = getTextForVisualTerm(term, knowledge);
  if (!text) {
    text = big(term.charAt(0).toUpperCase() + term.slice(1) + ".");
  }

  return {
    type: "media",
    html:
      "<p>" + text + "</p>" +
      '<div class="chat-image-wrap">' +
      '<img src="' + imageEntry.image + '" alt="' + imageEntry.alt + '" />' +
      '<span class="chat-image-caption">' + imageEntry.alt + "</span>" +
      "</div>"
  };
}

function isYearQuestion(question) {
  return (
    /(?:когда|в каком году)/.test(question) &&
    /(созда|изобр|запатент|постро|внедр|придум|изготов|основан|появил|запущен)/.test(question)
  );
}

function getYearAnswer(question, knowledge) {
  if (!isYearQuestion(question)) return "";

  var normalized = question.replace(/[?!.,;:]/g, " ").replace(/\s+/g, " ").trim();
  var candidates = [];

  for (var i = 0; i < knowledge.length; i++) {
    if (isServiceRow(knowledge[i])) continue;
    if (!/\d{4}\s*году/.test(knowledge[i][2])) continue;

    var subject = knowledge[i][0].toLowerCase();
    var rowText = (knowledge[i][0] + " " + knowledge[i][1] + " " + knowledge[i][2]).toLowerCase();
    var score = 0;

    if (/паров/.test(normalized) && /паров/.test(rowText)) score += 3;
    if (/уатт/.test(normalized) && /уатт/.test(rowText)) score += 6;
    if (/ньюкомен/.test(normalized) && /(ньюкомен|атмосферн)/.test(rowText)) score += 6;
    if (/севери/.test(normalized) && /(севери|первую практическ)/.test(rowText)) score += 6;
    if (/паровоз/.test(normalized) && /паровоз/.test(rowText)) score += 6;
    if (/пароход/.test(normalized) && /пароход/.test(rowText)) score += 6;
    if (/атмосферн/.test(normalized) && /атмосферн/.test(rowText)) score += 8;
    if (/практическ/.test(normalized) && /практическ/.test(rowText)) score += 5;
    if (/(созда|изобр|постро|запатент)/.test(normalized) && /(создал|создали|построил|изобрел|запатентовали|внедрили|запустили)/.test(knowledge[i][1])) {
      score += 2;
    }

    if (score > 0) {
      candidates.push({ score: score, idx: i });
    }
  }

  candidates.sort(function (a, b) { return b.score - a.score; });

  if (
    candidates.length > 1 &&
    /паров/.test(normalized) &&
    !/(уатт|ньюкомен|севери|паровоз|пароход|фултон|стефенсон|атмосферн|практическ|перв)/.test(normalized)
  ) {
    var timeline = [];
    var order = [
      "первую практическую паровую машину",
      "атмосферную паровую машину",
      "паровую машину уатта"
    ];
    for (var o = 0; o < order.length; o++) {
      for (var k = 0; k < knowledge.length; k++) {
        if (
          knowledge[k][0].toLowerCase() === order[o] &&
          /\d{4}\s*году/.test(knowledge[k][2])
        ) {
          timeline.push(formatRow(knowledge[k]).trim());
          break;
        }
      }
    }
    if (timeline.length) {
      return timeline.join(" ");
    }
  }

  if (candidates.length) {
    return formatRow(knowledge[candidates[0].idx]);
  }

  return "";
}

function hasWord(text, word) {
  return new RegExp("(^|\\s)" + word + "(\\s|$|[?!.,;:])").test(" " + text + " ");
}

function isDefinitionQuestion(question) {
  return (
    /(?:^|\s)(что|какое|какой|какая|какие)(?:\s|$)/.test(question) &&
    /(?:^|\s)(такое|такая|такой|такие|означает|значит)(?:\s|$)/.test(question)
  );
}

function getQuestionTerm(question) {
  var stopWords = {
    "что": true, "какое": true, "какой": true, "какая": true, "какие": true,
    "такое": true, "такая": true, "такой": true, "такие": true,
    "означает": true, "значит": true, "это": true
  };

  var words = question.split(" ");
  var termWords = [];
  var afterMarker = false;

  for (var i = 0; i < words.length; i++) {
    if (/^(такое|такая|такой|такие|означает|значит)$/.test(words[i])) {
      afterMarker = true;
      continue;
    }
    if (afterMarker && words[i].length > 1 && !stopWords[words[i]]) {
      termWords.push(words[i]);
    }
  }

  if (!termWords.length) {
    for (var j = words.length - 1; j >= 0; j--) {
      if (words[j].length > 2 && !stopWords[words[j]]) {
        termWords.push(words[j]);
        break;
      }
    }
  }

  return termWords.join(" ");
}

function getDefinitionAnswer(question, knowledge) {
  if (!isDefinitionQuestion(question)) return "";

  var term = getQuestionTerm(question);
  if (!term) return "";

  for (var i = 0; i < knowledge.length; i++) {
    if (knowledge[i][1] === "— это" && knowledge[i][0].toLowerCase() === term) {
      return formatRow(knowledge[i]);
    }
  }

  var bestScore = 0;
  var bestRow = null;
  for (var j = 0; j < knowledge.length; j++) {
    if (knowledge[j][1] !== "— это") continue;
    var subject = knowledge[j][0].toLowerCase();
    if (subject === term) {
      return formatRow(knowledge[j]);
    }
    if (subject.indexOf(term) !== -1 || term.indexOf(subject) !== -1) {
      var score = term.length / Math.max(subject.length, term.length);
      if (score > bestScore) {
        bestScore = score;
        bestRow = knowledge[j];
      }
    }
  }

  if (bestRow) {
    return formatRow(bestRow);
  }
  return "";
}

function getSpecialAnswer(question, knowledge) {
  var rows = [];

  function pickBest(candidates) {
    if (!candidates.length) return "";
    return rowsToAnswer([candidates[0]]);
  }

  var definitionAnswer = getDefinitionAnswer(question, knowledge);
  if (definitionAnswer) return definitionAnswer;

  if (/(из чего|из каких частей).*(состоит|состоит)/.test(question) && /паров/.test(question)) {
    for (var i = 0; i < knowledge.length; i++) {
      var subjectA = knowledge[i][0].toLowerCase();
      if (
        /(состоит|включает|содержит)/.test(knowledge[i][1]) &&
        /(паровая машина|установка паровой машины|основные части паровой машины|конструкция паровой машины)/.test(subjectA)
      ) {
        rows.push(knowledge[i]);
      }
    }
    if (rows.length) return pickBest(rows);
  }

  if (/(как|каким образом).*(устроен|устроена|устроено|устроены)/.test(question) && /паров/.test(question)) {
    for (var u = 0; u < knowledge.length; u++) {
      var subjectU = knowledge[u][0].toLowerCase();
      if (
        /(паровая машина|конструкция паровой машины|установка паровой машины)/.test(subjectU) &&
        /(состоит|содержит|включает)/.test(knowledge[u][1])
      ) {
        rows.push(knowledge[u]);
      }
    }
    if (rows.length) return pickBest(rows);
  }

  if (/(что).*(содержит|содержится)/.test(question) && /паров/.test(question)) {
    for (var s = 0; s < knowledge.length; s++) {
      var subjectS = knowledge[s][0].toLowerCase();
      if (
        /(содержит|содержится|включает|состоит)/.test(knowledge[s][1]) &&
        /(паровая машина|конструкция паровой машины|установка паровой машины|основные части паровой машины)/.test(subjectS)
      ) {
        rows.push(knowledge[s]);
      }
    }
    if (rows.length) return pickBest(rows);
  }

  if (hasWord(question, "кто") && /(создател|создал|придумал|изобр)/.test(question) && /паров/.test(question)) {
    for (var j = 0; j < knowledge.length; j++) {
      var rowTextB = (knowledge[j][0] + " " + knowledge[j][1] + " " + knowledge[j][2]).toLowerCase();
      if (
        /паров/.test(rowTextB) &&
        /(создал|придумал|изобрел|является)/.test(knowledge[j][1]) &&
        /(создателем паровой машины|паровую машину)/.test(knowledge[j][0].toLowerCase())
      ) {
        rows.push(knowledge[j]);
      }
    }
    if (rows.length) return pickBest(rows);
  }

  if (/(когда|в каком году).*(придум|созда|изобр|запатент|постро|внедр).*(паров)/.test(question)) {
    var yearFromSpecial = getYearAnswer(question, knowledge);
    if (yearFromSpecial) return yearFromSpecial;
  }

  if (/что делает пар(?:\s|$)/.test(question)) {
    for (var p = 0; p < knowledge.length; p++) {
      if (/^пар$/.test(knowledge[p][0].toLowerCase()) && /(создает|двигает|выполняет|поступает)/.test(knowledge[p][1])) {
        rows.push(knowledge[p]);
      }
    }
    if (rows.length) return pickBest(rows);
  }

  if (/что делает паровая машина/.test(question)) {
    for (var pm = 0; pm < knowledge.length; pm++) {
      if (/^паровая машина$/.test(knowledge[pm][0].toLowerCase()) && /(преобразует|делает|запускает|работает)/.test(knowledge[pm][1])) {
        rows.push(knowledge[pm]);
      }
    }
    if (rows.length) return pickBest(rows);
  }

  if (/(скорость маховика).*(вычис|высчит|счит)/.test(question)) {
    for (var m = 0; m < knowledge.length; m++) {
      if (/скорость маховика/.test(knowledge[m][0].toLowerCase()) && /(вычисляется|высчитывается|определяется)/.test(knowledge[m][1])) {
        rows.push(knowledge[m]);
      }
    }
    if (rows.length) return pickBest(rows);
  }

  if (/(?:^|\s)кпд(?:\s|$|[?!.,;:])/.test(question) && /(вычис|высчит|счит)/.test(question)) {
    for (var c = 0; c < knowledge.length; c++) {
      if (/^кпд$/.test(knowledge[c][0].toLowerCase()) && /(вычисляется|высчитывается|определяется)/.test(knowledge[c][1])) {
        rows.push(knowledge[c]);
      }
    }
    if (rows.length) return pickBest(rows);
  }

  if (/что такое кпд/.test(question)) {
    for (var d = 0; d < knowledge.length; d++) {
      if (knowledge[d][1] === "— это" && /^кпд$/.test(knowledge[d][0].toLowerCase())) {
        return formatRow(knowledge[d]);
      }
    }
  }

  if (/(как включить|как запустить).*(паровую машину)/.test(question)) {
    for (var q = 0; q < knowledge.length; q++) {
      if (/(пуск паровой машины|пуск машины|паровую машину)/.test(knowledge[q][0].toLowerCase()) && /(выполняется|начинается|включают)/.test(knowledge[q][1])) {
        rows.push(knowledge[q]);
      }
    }
    if (rows.length) return pickBest(rows);
  }

  if (/(какие|какой).*(метод).*(имитац|иммитац)/.test(question)) {
    for (var im = 0; im < knowledge.length; im++) {
      if (/методы им(м)?итационного моделирования/.test(knowledge[im][0].toLowerCase()) && /включают/.test(knowledge[im][1])) {
        rows.push(knowledge[im]);
      }
    }
    if (rows.length) return pickBest(rows);
  }

  return "";
}

function fallbackBySubject(words, knowledge, question) {
  if (isDefinitionQuestion(question)) {
    return getDefinitionAnswer(question, knowledge);
  }

  if (isYearQuestion(question)) {
    return getYearAnswer(question, knowledge);
  }

  var stopWords = {
    "кто": true, "что": true, "как": true, "где": true, "когда": true, "почему": true,
    "зачем": true, "какой": true, "какая": true, "какие": true, "какое": true,
    "ли": true, "это": true, "такое": true, "есть": true, "был": true, "была": true, "были": true
  };
  var meaningful = [];
  for (var i = 0; i < words.length; i++) {
    if (words[i].length > 2 && !stopWords[words[i]]) {
      meaningful.push(words[i]);
    }
  }
  if (!meaningful.length) return "";

  var term = meaningful.join(" ");
  var bestRow = null;
  var bestScore = -1;

  for (var j = 0; j < knowledge.length; j++) {
    if (isServiceRow(knowledge[j])) continue;

    var subject = knowledge[j][0].toLowerCase();
    var object = knowledge[j][2].toLowerCase();
    var score = 0;

    if (subject === term) score += 10;
    else if (subject.indexOf(term) !== -1) score += 5;
    else if (term.indexOf(subject) !== -1) score += 4;
    else if (object.indexOf(term) !== -1) score += 2;

    if (knowledge[j][1] === "— это") score += 3;

    if (score > bestScore) {
      bestScore = score;
      bestRow = knowledge[j];
    }
  }

  return bestRow && bestScore > 0 ? formatRow(bestRow) : "";
}

function scoredFallback(question, words, knowledge) {
  var stopWords = {
    "кто": true, "что": true, "как": true, "где": true, "когда": true, "почему": true,
    "зачем": true, "какой": true, "какая": true, "какие": true, "какое": true,
    "ли": true, "это": true, "такое": true, "есть": true, "был": true, "была": true, "были": true
  };

  var keywords = [];
  for (var i = 0; i < words.length; i++) {
    if (words[i].length > 2 && !stopWords[words[i]]) {
      keywords.push(words[i]);
    }
  }
  if (!keywords.length) return "";

  var askWho = hasWord(question, "кто");
  var askDefinition = isDefinitionQuestion(question);
  var askYear = isYearQuestion(question);

  var candidates = [];
  for (var j = 0; j < knowledge.length; j++) {
    if (isServiceRow(knowledge[j])) continue;
    var rowText = (knowledge[j][0] + " " + knowledge[j][1] + " " + knowledge[j][2]).toLowerCase();
    var score = 0;
    for (var k = 0; k < keywords.length; k++) {
      if (rowText.indexOf(keywords[k]) !== -1) score++;
    }

    if (askWho && /(создал|придумал|изобрел|ввел|усовершенствовал|внедрил|является)/.test(knowledge[j][1])) {
      score += 2;
    }
    if (askDefinition && (knowledge[j][1] === "— это" || /(является|это|представляет)/.test(knowledge[j][1]))) {
      score += 3;
    }
    if (askYear && /\d{4}\s*году/.test(knowledge[j][2])) {
      score += 5;
    }
    if (askYear && /(является|это)/.test(knowledge[j][1])) {
      score -= 3;
    }

    if (score >= 2) {
      candidates.push({ score: score, idx: j });
    }
  }

  candidates.sort(function (a, b) { return b.score - a.score; });
  if (!candidates.length) return "";

  return formatRow(knowledge[candidates[0].idx]);
}

function getAnswer(question) {
  var knowledge = knowleage;
  var result = false;
  var answer = "";

  if (!question || !question.trim().length) {
    return "Ответ не найден.";
  }

  question = small(question.trim());
  question = question.replace(/([,.!?;:()])/g, " $1 ");
  question = question.replace(/\s+/g, " ").trim();

  if (isVideoQuestion(question)) {
    return getVideoAnswer();
  }

  var imageAnswer = getImageAnswer(question, knowledge);
  if (imageAnswer) {
    return imageAnswer;
  }

  var yearAnswer = getYearAnswer(question, knowledge);
  if (yearAnswer) {
    return yearAnswer;
  }

  var specialAnswer = getSpecialAnswer(question, knowledge);
  if (specialAnswer) {
    return specialAnswer;
  }

  var words = question.split(" ");

  for (var i = 0; i < words.length; i++) {
    var ending = getEnding(words[i]);
    if (ending >= 0) {
      var originalWord = words[i];
      words[i] = words[i].substring(0, words[i].length - endings[ending][0].length) + endings[ending][1];
      var predicate = new RegExp(words[i]);

      if (/^придума/.test(originalWord) || /^изобр/.test(originalWord)) {
        predicate = new RegExp("(придумал|создал|изобрел|разработал|усовершенствовал)");
      }

      if (endings[ending][0] === endings[ending][1] && i + 1 < words.length) {
        predicate = new RegExp(words[i] + " " + words[i + 1]);
        i++;
      }

      var subjectString = words.slice(i + 1).join(".*");
      if (subjectString.length > 3) {
        var subject = new RegExp(".*" + subjectString + ".*");

        for (var j = 0; j < knowledge.length; j++) {
          if (isServiceRow(knowledge[j])) continue;
          if (
            predicate.test(knowledge[j][1]) &&
            (subject.test(knowledge[j][0]) || subject.test(knowledge[j][2]))
          ) {
            answer = formatRow(knowledge[j]);
            result = true;
            break;
          }
        }

        if (!result) {
          for (var k = 0; k < knowledge.length; k++) {
            if (isServiceRow(knowledge[k])) continue;
            if (subject.test(knowledge[k][0]) || subject.test(knowledge[k][2])) {
              answer = formatRow(knowledge[k]);
              result = true;
              break;
            }
          }
        }
      }
    }
  }

  if (!result) {
    var fallbackAnswer = scoredFallback(question, words, knowledge);
    if (!fallbackAnswer) {
      fallbackAnswer = fallbackBySubject(words, knowledge, question);
    }
    if (fallbackAnswer) {
      answer = fallbackAnswer;
      result = true;
    }
  }

  if (!result) {
    answer = "Ответ не найден.";
  }
  return answer;
}
