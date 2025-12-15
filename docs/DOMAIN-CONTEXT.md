# Parrhesia API

---

O sistema deverá ser capaz de atender milhões de usuários online e ser resiliente a picos de acesso. No entanto, não há expectativa de um grande número de usuários ativos simultaneamente: muitos usuários poderão acessar a pesquisa, mas apenas uma fração efetivamente responderá.

É crítico garantir que a aplicação seja entregue dentro do prazo estipulado (8 semanas), pois qualquer atraso colocaria em xeque a credibilidade da startup. Ainda assim, o sistema deve assegurar a consistência na contagem dos votos. Por se tratar de uma pesquisa amostral, a eventual perda de alguns votos é aceitável, desde que todos os votos efetivamente computados sejam corretos e consistentes.

Nosso principal objetivo é entregar uma solução funcional no menor tempo possível, considerando a proximidade das eleições. Por questões contratuais, a tecnologia adotada será C# com .NET Framework. O time é composto por cinco desenvolvedores, todos com experiência prévia nessas ferramentas.

Dado o prazo restrito e o tamanho da equipe, optaremos por uma arquitetura monolítica, que simplifica a manutenibilidade e reduz a complexidade operacional e de gerenciamento das diferentes partes do sistema.

Após o período de coleta de respostas, o sistema deverá disponibilizar, de forma sumarizada, os resultados da pesquisa para um conjunto restrito de usuários autorizados.